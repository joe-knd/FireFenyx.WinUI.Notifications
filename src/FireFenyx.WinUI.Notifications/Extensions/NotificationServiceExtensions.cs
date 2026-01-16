using FireFenyx.WinUI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Extensions;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddSingleton<INotificationQueue, NotificationQueue>();
        services.AddSingleton<INotificationService, NotificationService>();
        return services;
    }
}
