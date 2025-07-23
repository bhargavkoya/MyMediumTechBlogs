namespace CustomDIImplementation.AdvancedContainer
{
    public interface IAdvancedContainer : IDisposable
    {
        // Registration methods
        void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface;
        void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface;
        void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface;
        void RegisterSingleton<T>(T instance);
        void RegisterFactory<T>(Func<IAdvancedContainer, T> factory, ServiceLifetime lifetime = ServiceLifetime.Transient);

        // Resolution methods
        T GetService<T>();
        object GetService(Type serviceType);
        T GetRequiredService<T>();
        object GetRequiredService(Type serviceType);

        // Scope management
        IServiceScope CreateScope();
        void ClearScoped();

        // Validation
        void ValidateRegistrations();
    }
}
