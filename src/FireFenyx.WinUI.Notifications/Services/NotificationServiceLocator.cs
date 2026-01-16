using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Services;

public static class NotificationServiceLocator
{
    public static IServiceProvider? Services { get; private set; }

    public static void Initialize(IServiceProvider provider)
        => Services = provider;
}
