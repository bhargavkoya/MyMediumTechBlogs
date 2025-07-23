namespace CustomDIImplementation.AdvancedContainer
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public object Instance { get; set; }
        public Func<IAdvancedContainer, object> Factory { get; set; }
        public bool IsFactoryRegistration => Factory != null;
    }

    public enum ServiceLifetime
    {
        Transient,
        Scoped,
        Singleton
    }
}
