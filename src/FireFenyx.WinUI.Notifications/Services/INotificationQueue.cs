using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Threading.Tasks;

namespace FireFenyx.WinUI.Notifications.Services;

/// <summary>
/// Represents a queue that accepts <see cref="NotificationRequest"/> objects and processes them sequentially.
/// </summary>
public interface INotificationQueue
{
    /// <summary>
    /// Enqueues a notification request.
    /// </summary>
    /// <param name="request">The request to enqueue.</param>
    void Enqueue(NotificationRequest request);

    /// <summary>
    /// Sets the processor function that will be invoked for each dequeued notification.
    /// </summary>
    /// <param name="processor">The processor function.</param>
    void SetProcessor(Func<NotificationRequest, Task> processor);

}
