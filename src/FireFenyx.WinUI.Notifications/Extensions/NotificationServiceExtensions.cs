using FireFenyx.WinUI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FireFenyx.WinUI.Notifications.Extensions;

/// <summary>
/// Extension methods for registering FireFenyx notification services.
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// Registers the notification queue and service into the provided DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddSingleton<INotificationQueue, NotificationQueue>();
        services.AddSingleton<INotificationService, NotificationService>();
        return services;
    }
}
