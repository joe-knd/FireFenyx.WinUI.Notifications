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

    public void Update(NotificationRequest request)
        => _queue.Enqueue(new NotificationRequest
        {
            Id = request.Id,
            IsUpdate = true,
            Message = request.Message,
            Level = request.Level,
            DurationMs = request.DurationMs,
            ActionText = request.ActionText,
            Action = request.Action,
            IsInProgress = request.IsInProgress,
            Progress = request.Progress,
            Transition = request.Transition,
            Material = request.Material
        });

    public IProgressNotification ShowProgress(string message, int durationMs = 3000, double progress = -1)
    {
        var id = Guid.NewGuid();
        Show(new NotificationRequest
        {
            Id = id,
            Message = message,
            Level = NotificationLevel.Info,
            IsInProgress = true,
            Progress = progress,
            DurationMs = durationMs
        });

        return new ProgressNotification(this, id, durationMs);
    }

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

    private sealed class ProgressNotification : IProgressNotification
    {
        private readonly NotificationService _service;
        private readonly int _durationMs;

        public ProgressNotification(NotificationService service, Guid id, int durationMs)
        {
            _service = service;
            Id = id;
            _durationMs = durationMs;
        }

        public Guid Id { get; }

        public void Report(double progress, string? message = null)
            => _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message ?? string.Empty,
                Level = NotificationLevel.Info,
                IsInProgress = true,
                Progress = progress,
                DurationMs = _durationMs
            });

        public void Complete(string? message = null)
            => _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message ?? string.Empty,
                Level = NotificationLevel.Success,
                IsInProgress = false,
                Progress = 100,
                DurationMs = _durationMs
            });
    }
}
