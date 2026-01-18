using FireFenyx.WinUI.Notifications.Models;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FireFenyx.WinUI.Notifications.Services;

/// <summary>
/// Default implementation of <see cref="INotificationQueue"/> based on <see cref="Channel{T}"/>.
/// </summary>
public sealed class NotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationRequest> _channel =
        Channel.CreateUnbounded<NotificationRequest>();

    private Func<NotificationRequest, Task>? _processor;
    private readonly DispatcherQueue? _dispatcherQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationQueue"/> class.
    /// </summary>
    public NotificationQueue()
    {
        // Resolving this service should happen on the UI thread in typical WinUI apps.
        // If we have a DispatcherQueue, we can always marshal UI work back correctly.
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        Task.Run(ProcessLoopAsync);
    }

    /// <inheritdoc />
    public void SetProcessor(Func<NotificationRequest, Task> processor)
        => _processor = processor;

    /// <inheritdoc />
    public void Enqueue(NotificationRequest request) 
        => _channel.Writer.TryWrite(request);

    private static Task EnqueueOnDispatcherAsync(DispatcherQueue dispatcherQueue, Func<Task> action)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await action().ConfigureAwait(true);
                tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }))
        {
            tcs.TrySetException(new InvalidOperationException("Failed to enqueue notification processing on the UI DispatcherQueue."));
        }

        return tcs.Task;
    }

    private async Task ProcessLoopAsync()
    {
        await foreach (var request in _channel.Reader.ReadAllAsync())
        {
            if (_processor is not null)
            {
                if (_dispatcherQueue is null || _dispatcherQueue.HasThreadAccess)
                {
                    await _processor(request);
                }
                else
                {
                    await EnqueueOnDispatcherAsync(_dispatcherQueue, () => _processor(request)).ConfigureAwait(false);
                }
            }
        }
    }
}
