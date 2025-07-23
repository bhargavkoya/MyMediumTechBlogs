namespace CustomDIImplementation.SimpleDIContainer
{
    // Simple custom DI container implementation
    public class SimpleContainer : IServiceContainer
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services = new();
        private readonly Dictionary<Type, object> _singletonInstances = new();
        private readonly Dictionary<Type, object> _scopedInstances = new();

        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Transient
            };
        }

        public void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Scoped
            };
        }

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Singleton
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
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
            }

            return serviceDescriptor.Lifetime switch
            {
                ServiceLifetime.Singleton => GetSingleton(serviceDescriptor),
                ServiceLifetime.Scoped => GetScoped(serviceDescriptor),
                ServiceLifetime.Transient => CreateInstance(serviceDescriptor.ImplementationType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private object GetSingleton(ServiceDescriptor serviceDescriptor)
        {
            if (_singletonInstances.TryGetValue(serviceDescriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(serviceDescriptor.ImplementationType);
            _singletonInstances[serviceDescriptor.ServiceType] = instance;
            return instance;
        }

        private object GetScoped(ServiceDescriptor serviceDescriptor)
        {
            if (_scopedInstances.TryGetValue(serviceDescriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(serviceDescriptor.ImplementationType);
            _scopedInstances[serviceDescriptor.ServiceType] = instance;
            return instance;
        }

        private object CreateInstance(Type implementationType)
        {
            var constructors = implementationType.GetConstructors();
            var constructor = constructors[0]; // Simplified: take first constructor

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = GetService(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(implementationType, args);
        }

        // Method to clear scoped instances (typically called at end of request)
        public void ClearScoped()
        {
            _scopedInstances.Clear();
        }
    }
}
