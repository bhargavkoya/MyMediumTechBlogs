namespace CustomDIImplementation.SimpleDIContainer
{
    public interface IServiceContainer
    {
        void RegisterTransient<TInterface, TImplementation>()
           where TImplementation : class, TInterface;
        void RegisterScoped<TInterface, TImplementation>()
            where TImplementation : class, TInterface;
        void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface;
        T GetService<T>();
        object GetService(Type serviceType);
    }
}
