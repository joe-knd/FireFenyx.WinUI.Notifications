using FireFenyx.WinUI.Notifications.Controls;
using Microsoft.UI.Xaml;

namespace FireFenyx.WinUI.Notifications.Services;

/// <summary>
/// Provides an attached property used to connect an <see cref="INotificationQueue"/> to a <see cref="Controls.NotificationHost"/>.
/// </summary>
public static class NotificationHostService
{
    /// <summary>
    /// Identifies the attached <c>Queue</c> dependency property.
    /// </summary>
    public static readonly DependencyProperty QueueProperty =
        DependencyProperty.RegisterAttached(
        "Queue",
        typeof(INotificationQueue),
        typeof(NotificationHostService),
        new PropertyMetadata(null, OnQueueChanged));

    /// <summary>
    /// Sets the notification queue for a <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <param name="value">The queue to set.</param>
    public static void SetQueue(DependencyObject element, INotificationQueue? value) 
        => element.SetValue(QueueProperty, value);

    /// <summary>
    /// Gets the notification queue for a <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="element">The target element.</param>
    /// <returns>The configured queue, or <see langword="null"/>.</returns>
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
