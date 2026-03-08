using FireFenyx.Notifications.Models;
using FireFenyx.Notifications.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace FireFenyx.Wpf.Notifications.Tests;

public sealed class NotificationServiceTests
{
    private sealed class CapturingQueue : INotificationQueue
    {
        public List<NotificationRequest> All { get; } = [];
        public NotificationRequest? Last => All.Count > 0 ? All[^1] : null;

        public void Enqueue(NotificationRequest request) => All.Add(request);
        public void SetProcessor(Func<NotificationRequest, System.Threading.Tasks.Task> processor) { }
    }

    [Fact]
    public void Show_ShouldEnqueueRequest()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);
        var request = new NotificationRequest { Message = "hello" };

        svc.Show(request);

        Assert.Single(queue.All);
        Assert.Same(request, queue.Last);
    }

    [Fact]
    public void Update_ShouldSetIsUpdateFlag()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);
        var id = Guid.NewGuid();

        svc.Update(new NotificationRequest { Id = id, Message = "updated" });

        Assert.NotNull(queue.Last);
        Assert.Equal(id, queue.Last!.Id);
        Assert.True(queue.Last.IsUpdate);
        Assert.Equal("updated", queue.Last.Message);
    }

    [Fact]
    public void Info_ShouldEnqueueWithInfoLevel()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        svc.Info("info msg", durationMs: 5000);

        Assert.NotNull(queue.Last);
        Assert.Equal("info msg", queue.Last!.Message);
        Assert.Equal(NotificationLevel.Info, queue.Last.Level);
        Assert.Equal(5000, queue.Last.DurationMs);
    }

    [Fact]
    public void Success_ShouldEnqueueWithSuccessLevel()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        svc.Success("done");

        Assert.NotNull(queue.Last);
        Assert.Equal("done", queue.Last!.Message);
        Assert.Equal(NotificationLevel.Success, queue.Last.Level);
    }

    [Fact]
    public void Warning_ShouldEnqueueWithWarningLevel()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        svc.Warning("warn");

        Assert.NotNull(queue.Last);
        Assert.Equal("warn", queue.Last!.Message);
        Assert.Equal(NotificationLevel.Warning, queue.Last.Level);
    }

    [Fact]
    public void Error_ShouldEnqueueWithErrorLevel()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        svc.Error("fail");

        Assert.NotNull(queue.Last);
        Assert.Equal("fail", queue.Last!.Message);
        Assert.Equal(NotificationLevel.Error, queue.Last.Level);
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

    [Fact]
    public void ShowProgress_Determinate_ShouldUseProvidedValue()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowProgress("loading", progress: 25);

        Assert.NotNull(queue.Last);
        Assert.True(queue.Last!.IsInProgress);
        Assert.Equal(25, queue.Last.Progress);
        Assert.Equal("loading", queue.Last.Message);
        Assert.NotEqual(Guid.Empty, handle.Id);
    }

    [Fact]
    public void ProgressNotification_Report_ShouldUpdateProgress()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowProgress("start");
        handle.Report(50, "halfway");

        Assert.Equal(2, queue.All.Count);
        var update = queue.Last!;
        Assert.Equal(handle.Id, update.Id);
        Assert.True(update.IsUpdate);
        Assert.True(update.IsInProgress);
        Assert.Equal(50, update.Progress);
        Assert.Equal("halfway", update.Message);
    }

    [Fact]
    public void ProgressNotification_Indeterminate_ShouldSetNegativeProgress()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowProgress("start");
        handle.Indeterminate("waiting");

        Assert.Equal(2, queue.All.Count);
        var update = queue.Last!;
        Assert.True(update.IsInProgress);
        Assert.True(update.Progress < 0);
        Assert.Equal("waiting", update.Message);
    }

    [Fact]
    public void ProgressNotification_Complete_ShouldMarkDone()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowProgress("working");
        handle.Complete("finished");

        Assert.Equal(2, queue.All.Count);
        var update = queue.Last!;
        Assert.Equal(handle.Id, update.Id);
        Assert.True(update.IsUpdate);
        Assert.False(update.IsInProgress);
        Assert.Equal(100, update.Progress);
        Assert.Equal(NotificationLevel.Success, update.Level);
        Assert.Equal("finished", update.Message);
    }

    [Fact]
    public void ShowPersistent_ShouldEnqueueWithZeroDuration()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowPersistent("sticky", NotificationLevel.Warning, isClosable: true);

        Assert.NotNull(queue.Last);
        Assert.Equal(handle.Id, queue.Last!.Id);
        Assert.Equal("sticky", queue.Last.Message);
        Assert.Equal(NotificationLevel.Warning, queue.Last.Level);
        Assert.Equal(0, queue.Last.DurationMs);
        Assert.True(queue.Last.IsClosable);
    }

    [Fact]
    public void PersistentNotification_Update_ShouldSendUpdateRequest()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowPersistent("initial");
        handle.Update("changed", NotificationLevel.Error, durationMs: 5000);

        Assert.Equal(2, queue.All.Count);
        var update = queue.Last!;
        Assert.Equal(handle.Id, update.Id);
        Assert.True(update.IsUpdate);
        Assert.Equal("changed", update.Message);
        Assert.Equal(NotificationLevel.Error, update.Level);
        Assert.Equal(5000, update.DurationMs);
    }

    [Fact]
    public void PersistentNotification_Dismiss_ShouldSendDismissRequest()
    {
        var queue = new CapturingQueue();
        var svc = new NotificationService(queue);

        var handle = svc.ShowPersistent("temp");
        handle.Dismiss();

        Assert.Equal(2, queue.All.Count);
        var update = queue.Last!;
        Assert.Equal(handle.Id, update.Id);
        Assert.True(update.IsUpdate);
        Assert.True(update.DismissRequested);
    }
}
