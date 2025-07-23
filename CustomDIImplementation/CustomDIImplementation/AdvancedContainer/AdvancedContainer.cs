using System.Collections.Concurrent;
using System.Reflection;

namespace CustomDIImplementation.AdvancedContainer
{
    public class AdvancedContainer : IAdvancedContainer
    {
        private readonly ConcurrentDictionary<Type, ServiceDescriptor> _services = new();

        private readonly ConcurrentDictionary<Type, object> _singletonInstances = new();
        private readonly ThreadLocal<Dictionary<Type, object>> _scopedInstances =
            new ThreadLocal<Dictionary<Type, object>>(() => new Dictionary<Type, object>());

        private readonly ConcurrentDictionary<Type, Func<object[], object>> _compiledFactories = new();
        private readonly ThreadLocal<HashSet<Type>> _resolutionStack =
            new ThreadLocal<HashSet<Type>>(() => new HashSet<Type>());

        private bool _disposed = false;

        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            RegisterService<TInterface, TImplementation>(ServiceLifetime.Transient);
        }

        public void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            RegisterService<TInterface, TImplementation>(ServiceLifetime.Scoped);
        }

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            RegisterService<TInterface, TImplementation>(ServiceLifetime.Singleton);
        }

        public void RegisterSingleton<T>(T instance)
        {
            _services[typeof(T)] = new ServiceDescriptor
            {
                ServiceType = typeof(T),
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance
            };
            _singletonInstances[typeof(T)] = instance;
        }

        public void RegisterFactory<T>(Func<IAdvancedContainer, T> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            _services[typeof(T)] = new ServiceDescriptor
            {
                ServiceType = typeof(T),
                Lifetime = lifetime,
                Factory = container => factory(container)
            };
        }

        private void RegisterService<TInterface, TImplementation>(ServiceLifetime lifetime)
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = lifetime
            };
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (!_services.TryGetValue(serviceType, out var serviceDescriptor))
            {
                return null; // Return null for optional services
            }

            // Circular dependency detection
            if (_resolutionStack.Value.Contains(serviceType))
            {
                var chain = string.Join(" -> ", _resolutionStack.Value.Select(t => t.Name)) + " -> " + serviceType.Name;
                throw new CircularDependencyException($"Circular dependency detected: {chain}");
            }

            _resolutionStack.Value.Add(serviceType);

            try
            {
                return ResolveService(serviceDescriptor);
            }
            finally
            {
                _resolutionStack.Value.Remove(serviceType);
            }
        }

        public T GetRequiredService<T>()
        {
            var service = GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
            }
            return service;
        }

        public object GetRequiredService(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
            }
            return service;
        }

        private object ResolveService(ServiceDescriptor descriptor)
        {
            // Handle pre-registered instances
            if (descriptor.Instance != null)
            {
                return descriptor.Instance;
            }

            // Handle factory registrations
            if (descriptor.IsFactoryRegistration)
            {
                return descriptor.Factory(this);
            }

            return descriptor.Lifetime switch
            {
                ServiceLifetime.Singleton => GetSingleton(descriptor),
                ServiceLifetime.Scoped => GetScoped(descriptor),
                ServiceLifetime.Transient => CreateInstance(descriptor.ImplementationType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private object GetSingleton(ServiceDescriptor descriptor)
        {
            return _singletonInstances.GetOrAdd(descriptor.ServiceType,
                _ => CreateInstance(descriptor.ImplementationType));
        }

        private object GetScoped(ServiceDescriptor descriptor)
        {
            var scopedInstances = _scopedInstances.Value;
            if (scopedInstances.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(descriptor.ImplementationType);
            scopedInstances[descriptor.ServiceType] = instance;
            return instance;
        }

        private object CreateInstance(Type implementationType)
        {
            // Try to use compiled factory for performance
            if (_compiledFactories.TryGetValue(implementationType, out var compiledFactory))
            {
                var param = GetConstructorParameters(implementationType);
                return compiledFactory(param);
            }

            // Fallback to reflection-based creation
            var constructors = implementationType.GetConstructors()
                .Where(c => c.IsPublic)
                .OrderByDescending(c => c.GetParameters().Length)
                .ToArray();

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructors found for {implementationType.Name}");
            }

            var constructor = constructors[0];
            var parameters = GetConstructorParameters(implementationType, constructor);

            return Activator.CreateInstance(implementationType, parameters);
        }

        private object[] GetConstructorParameters(Type type, ConstructorInfo constructor = null)
        {
            constructor ??= type.GetConstructors()[0];
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                args[i] = GetService(parameterType) ??
                         throw new InvalidOperationException(
                             $"Unable to resolve service for type {parameterType.Name} when attempting to activate {type.Name}.");
            }

            return args;
        }

        public void ValidateRegistrations()
        {
            foreach (var kvp in _services)
            {
                try
                {
                    GetService(kvp.Key);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Validation failed for service {kvp.Key.Name}: {ex.Message}", ex);
                }
            }
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScope(this);
        }

        public void ClearScoped()
        {
            _scopedInstances.Value?.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Dispose all singleton instances
                foreach (var instance in _singletonInstances.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _singletonInstances.Clear();
                _services.Clear();
                _compiledFactories.Clear();

                _scopedInstances?.Dispose();
                _resolutionStack?.Dispose();

                _disposed = true;
            }
        }
    }
}
