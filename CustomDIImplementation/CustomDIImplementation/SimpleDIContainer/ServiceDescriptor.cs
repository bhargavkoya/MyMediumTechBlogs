namespace CustomDIImplementation.SimpleDIContainer
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public object Instance { get; set; }
        public Func<IServiceContainer, object> Factory { get; set; }
    }

    // Service lifetime enumeration
    public enum ServiceLifetime
    {
        Transient,
        Scoped,
        Singleton
    }
}
