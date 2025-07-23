namespace CustomDIImplementation.AdvancedContainer
{
    public class ServiceScope : IServiceScope
    {
        private readonly IAdvancedContainer _container;
        private readonly Dictionary<Type, object> _scopedInstances = new();
        private bool _disposed = false;

        public ServiceScope(IAdvancedContainer container)
        {
            _container = container;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (_scopedInstances.TryGetValue(serviceType, out var instance))
            {
                return instance;
            }

            instance = _container.GetService(serviceType);
            if (instance != null)
            {
                _scopedInstances[serviceType] = instance;
            }

            return instance;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var instance in _scopedInstances.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _scopedInstances.Clear();
                _disposed = true;
            }
        }
    }
}
