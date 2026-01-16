using FireFenyx.WinUI.Notifications.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Services;

public static class NotificationHostService
{
    public static readonly DependencyProperty QueueProperty =
        DependencyProperty.RegisterAttached(
        "Queue",
        typeof(INotificationQueue),
        typeof(NotificationHostService),
        new PropertyMetadata(null, OnQueueChanged));

    public static void SetQueue(DependencyObject element, INotificationQueue? value) 
        => element.SetValue(QueueProperty, value);

    public static INotificationQueue? GetQueue(DependencyObject element) 
        => (INotificationQueue?)element.GetValue(QueueProperty);

    private static void OnQueueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationHost host && e.NewValue is INotificationQueue queue)
        {
            queue.SetProcessor(host.ShowAsync);
        }
    }
}
