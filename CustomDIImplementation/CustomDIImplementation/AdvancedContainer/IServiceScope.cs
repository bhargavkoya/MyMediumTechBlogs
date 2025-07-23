namespace CustomDIImplementation.AdvancedContainer
{
    public interface IServiceScope : IDisposable
    {
        T GetService<T>();
        object GetService(Type serviceType);
    }
}
