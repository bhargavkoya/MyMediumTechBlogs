namespace DependencyInjectionInNETCore.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message, string recipient);
        Task SendNotificationAsync(string message, string recipient, Dictionary<string, string> additionalData);
    }
}
