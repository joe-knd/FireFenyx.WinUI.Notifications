using FireFenyx.WinUI.Notifications.Models;
using System;

namespace FireFenyx.WinUI.Notifications.Services;

/// <summary>
/// Default implementation of <see cref="INotificationService"/> that enqueues requests into an <see cref="INotificationQueue"/>.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationQueue _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="queue">The queue used to dispatch notifications.</param>
    public NotificationService(INotificationQueue queue)
    {
        _queue = queue;
    }

    /// <inheritdoc />
    public void Show(NotificationRequest request)
        => _queue.Enqueue(request);

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Info(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Info,
            DurationMs = durationMs
        });

    /// <inheritdoc />
    public void Success(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Success,
            DurationMs = durationMs
        });

    /// <inheritdoc />
    public void Warning(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Warning,
            DurationMs = durationMs
        });

    /// <inheritdoc />
    public void Error(string message, int durationMs = 3000)
        => Show(new NotificationRequest
        {
            Message = message,
            Level = NotificationLevel.Error,
            DurationMs = durationMs
        });

    /// <inheritdoc />
    public IPersistentNotification ShowPersistent(string message, NotificationLevel level = NotificationLevel.Info, bool isClosable = false)
    {
        var id = Guid.NewGuid();
        Show(new NotificationRequest
        {
            Id = id,
            Message = message,
            Level = level,
            DurationMs = 0,
            IsClosable = isClosable
        });

        return new PersistentNotification(this, id, level, message);
    }

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

    private sealed class PersistentNotification : IPersistentNotification
    {
        private readonly NotificationService _service;

        public PersistentNotification(NotificationService service, Guid id, NotificationLevel level, string message)
        {
            _service = service;
            Id = id;
            Level = level;
            Message = message;
        }

        public Guid Id { get; }
        private NotificationLevel Level { get; set; }
        private string Message { get; set; }

        public void Update(string? message = null, NotificationLevel? level = null, int? durationMs = null)
        {
            if (message is not null)
            {
                Message = message;
            }

            if (level is not null)
            {
                Level = level.Value;
            }

            _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message ?? string.Empty,
                Level = level ?? Level,
                DurationMs = durationMs ?? 0
            });
        }

        public void Dismiss()
            => _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = string.Empty,
                Level = Level,
                DurationMs = 1
            });
    }
}
