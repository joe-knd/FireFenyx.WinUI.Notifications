using FireFenyx.WinUI.Notifications.Models;
using FireFenyx.WinUI.Notifications.Services;
using System;
using Xunit;

namespace FireFenyx.WinUI.Notifications.Tests;

public sealed class NotificationServiceTests
{
    private sealed class CapturingQueue : INotificationQueue
    {
        public NotificationRequest? Last { get; private set; }

        public void Enqueue(NotificationRequest request) => Last = request;
        public void SetProcessor(Func<NotificationRequest, System.Threading.Tasks.Task> processor) { }
    }

    [Fact]
    public void Dismiss_ShouldSendDismissRequestedUpdate()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var id = Guid.NewGuid();
        svc.Dismiss(id);

        Assert.NotNull(queue.Last);
        Assert.Equal(id, queue.Last!.Id);
        Assert.True(queue.Last.DismissRequested);
        Assert.True(queue.Last.IsUpdate);
    }

    [Fact]
    public void DismissAll_ShouldUseEmptyGuidSentinel()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        svc.DismissAll();

        Assert.NotNull(queue.Last);
        Assert.Equal(Guid.Empty, queue.Last!.Id);
        Assert.True(queue.Last.DismissRequested);
        Assert.True(queue.Last.IsUpdate);
    }

    [Fact]
    public void ShowProgress_Indeterminate_ShouldUseNegativeProgress()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        _ = svc.ShowProgress("x", progress: -1);

        Assert.NotNull(queue.Last);
        Assert.True(queue.Last!.IsInProgress);
        Assert.True(queue.Last.Progress < 0);
    }
}
