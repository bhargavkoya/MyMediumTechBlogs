namespace ECommerceWithRoutingAndMiddleware.Services
{
    public interface ICacheService
    {
        Task PingAsync();
    }

    public class CacheService : ICacheService
    {
        public Task PingAsync()
        {
            // Simulate a cache ping operation
            return Task.CompletedTask;
        }
    }
}