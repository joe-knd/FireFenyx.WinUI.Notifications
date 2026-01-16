using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationQueue _queue;

    public NotificationService(INotificationQueue queue)
    {
        _queue = queue;
    }

    public void Show(NotificationRequest request)
        => _queue.Enqueue(request);

    public void Info(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Info,
            DurationMs = durationMs
        });

    public void Success(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Success,
            DurationMs = durationMs
        });

    public void Warning(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Warning,
            DurationMs = durationMs
        });

    public void Error(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Error,
            DurationMs = durationMs
        });
}
