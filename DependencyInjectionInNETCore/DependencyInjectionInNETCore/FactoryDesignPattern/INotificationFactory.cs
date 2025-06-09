using DependencyInjectionInNETCore.Interfaces;
using DependencyInjectionInNETCore.Services;
using static DependencyInjectionInNETCore.FactoryDesignPattern.NotificationFactory;

namespace DependencyInjectionInNETCore.FactoryDesignPattern
{
    //factory interface
    public interface INotificationFactory
    {
        INotificationService CreateNotificationService(NotificationType type);
    }

    //factory implementation
    public class NotificationFactory : INotificationFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INotificationService CreateNotificationService(NotificationType type)
        {
            return type switch
            {
                NotificationType.Email => _serviceProvider.GetRequiredService<EmailNotificationService>(),
                NotificationType.SMS => _serviceProvider.GetRequiredService<SmsNotificationService>(),
                NotificationType.Push => _serviceProvider.GetRequiredService<PushNotificationService>(),
                _ => throw new ArgumentException($"Unknown notification type: {type}")
            };
        }

        public enum NotificationType
        {
            Email,
            SMS,
            Push
        }
    }
}
