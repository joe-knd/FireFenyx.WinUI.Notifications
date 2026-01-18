using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            IsClosable = request.IsClosable,
            DismissRequested = request.DismissRequested,
            ActionText = request.ActionText,
            Action = request.Action,
            ActionCommand = request.ActionCommand,
            ActionCommandParameter = request.ActionCommandParameter,
            IsInProgress = request.IsInProgress,
            Progress = request.Progress,
            Transition = request.Transition,
            Material = request.Material
        });

    /// <inheritdoc />
    public ICountdownNotification ShowCountdown(string title, TimeSpan duration, NotificationLevel level = NotificationLevel.Info, string? completionMessage = null, int updateIntervalMs = 1000)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero.");
        }

        if (updateIntervalMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(updateIntervalMs), "Update interval must be greater than zero.");
        }

        var id = Guid.NewGuid();
        var defaultCompletion = completionMessage ?? $"{title} completed.";
        var countdown = new CountdownNotification(this, id, defaultCompletion);

        Show(new NotificationRequest
        {
            Id = id,
            Message = FormatCountdownMessage(title, duration),
            Level = level,
            IsInProgress = true,
            Progress = 0,
            DurationMs = 0
        });

        _ = RunCountdownAsync(countdown, title, duration, level, defaultCompletion, updateIntervalMs);

        return countdown;
    }

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

        public void Indeterminate(string? message = null)
            => Report(-1, message);

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
                DurationMs = 0,
                DismissRequested = true
            });
    }

    private async Task RunCountdownAsync(CountdownNotification handle, string title, TimeSpan duration, NotificationLevel level, string completionMessage, int updateIntervalMs)
    {
        var totalMs = Math.Max(duration.TotalMilliseconds, 1);
        var interval = TimeSpan.FromMilliseconds(updateIntervalMs);
        var start = DateTimeOffset.UtcNow;

        try
        {
            while (!handle.Token.IsCancellationRequested)
            {
                var elapsed = DateTimeOffset.UtcNow - start;
                var remaining = duration - elapsed;

                if (remaining <= TimeSpan.Zero)
                {
                    handle.CompleteFromTimer(completionMessage);
                    return;
                }

                var progress = Math.Clamp(elapsed.TotalMilliseconds / totalMs * 100d, 0, 100);

                Update(new NotificationRequest
                {
                    Id = handle.Id,
                    Message = FormatCountdownMessage(title, remaining),
                    Level = level,
                    IsInProgress = true,
                    Progress = progress,
                    DurationMs = 0
                });

                await Task.Delay(interval, handle.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the countdown is cancelled or completed manually.
        }
        catch (ObjectDisposedException)
        {
            // Expected when the CTS is disposed while Task.Delay is awaiting.
        }
    }

    private static string FormatCountdownMessage(string title, TimeSpan remaining)
    {
        if (remaining < TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
        }

        return $"{title} ({FormatRemaining(remaining)} remaining)";
    }

    private static string FormatRemaining(TimeSpan remaining)
    {
        remaining = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;

        if (remaining.TotalHours >= 1)
        {
            var hours = (int)remaining.TotalHours;
            return $"{hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    private sealed class CountdownNotification : ICountdownNotification
    {
        private readonly NotificationService _service;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _defaultCompletionMessage;
        private int _terminalState;

        public CountdownNotification(NotificationService service, Guid id, string defaultCompletionMessage)
        {
            _service = service;
            Id = id;
            _defaultCompletionMessage = defaultCompletionMessage;
        }

        public Guid Id { get; }

        internal CancellationToken Token => _cts.Token;

        private bool TryBeginTerminalState()
            => Interlocked.Exchange(ref _terminalState, 1) == 0;

        public void Cancel(string? message = null)
        {
            if (!TryBeginTerminalState())
            {
                return;
            }

            _cts.Cancel();
            _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message ?? "Countdown canceled.",
                Level = NotificationLevel.Warning,
                IsInProgress = false,
                Progress = 0,
                DurationMs = 2500
            });

            _cts.Dispose();
        }

        public void Complete(string? message = null)
        {
            if (!TryBeginTerminalState())
            {
                return;
            }

            _cts.Cancel();
            _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message ?? _defaultCompletionMessage,
                Level = NotificationLevel.Success,
                IsInProgress = false,
                Progress = 100,
                DurationMs = 2500
            });

            _cts.Dispose();
        }

        internal void CompleteFromTimer(string message)
        {
            if (!TryBeginTerminalState())
            {
                return;
            }

            _service.Update(new NotificationRequest
            {
                Id = Id,
                Message = message,
                Level = NotificationLevel.Success,
                IsInProgress = false,
                Progress = 100,
                DurationMs = 2500
            });

            _cts.Dispose();
        }
    }
}
