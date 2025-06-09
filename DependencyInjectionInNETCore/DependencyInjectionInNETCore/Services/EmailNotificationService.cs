using DependencyInjectionInNETCore.Interfaces;

namespace DependencyInjectionInNETCore.Services
{
    public class EmailNotificationService : INotificationService
    {
        public Task SendNotificationAsync(string message, string recipient)
        {
            throw new NotImplementedException();
        }

        public Task SendNotificationAsync(string message, string recipient, Dictionary<string, string> additionalData)
        {
            throw new NotImplementedException();
        }
    }
}
