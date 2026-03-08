using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FireFenyx.Notifications.Models;
using FireFenyx.Notifications.SampleApp.Services;
using FireFenyx.Notifications.Services;

namespace FireFenyx.Notifications.SampleApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INotificationService _notifications;
    private readonly IDialogService _dialogs;
    private IPersistentNotification? _persistent;

    private CancellationTokenSource? _sendCts;
    private volatile bool _sendPaused;

    public MainViewModel(INotificationService notifications, IDialogService dialogs)
    {
        _notifications = notifications;
        _dialogs = dialogs;
    }

    public NotificationBarStyle[] BarStyleOptions { get; } = Enum.GetValues<NotificationBarStyle>();

    [ObservableProperty]
    private NotificationBarStyle _selectedBarStyle = NotificationBarStyle.Fluent;

    [RelayCommand]
    private void ShowSuccess()
        => _notifications.Success("Operation completed successfully!");

    [RelayCommand]
    private void ShowWarning()
        => _notifications.Warning("This is a warning message.");

    [RelayCommand]
    private void ShowError()
        => _notifications.Error("Something went wrong!", durationMs: 5000);

    [RelayCommand]
    private void ShowProgress()
    {
        var handle = _notifications.ShowProgress("Uploading...", durationMs: 1500, progress: 0);

        _ = Task.Run(async () =>
        {
            for (var i = 0; i <= 100; i += 10)
            {
                handle.Report(i, $"Uploading... {i}%");
                await Task.Delay(500);
            }

            handle.Complete("Upload completed!");
        });
    }

    [RelayCommand]
    private void SendFileComplexScenario()
    {
        _sendCts?.Cancel();
        _sendCts?.Dispose();
        _sendCts = new CancellationTokenSource();
        var token = _sendCts.Token;
        _sendPaused = false;

        var id = Guid.NewGuid();
        _notifications.Show(new NotificationRequest
        {
            Id = id,
            Message = "Sending file...",
            Level = NotificationLevel.Info,
            IsInProgress = true,
            Progress = -1,
            DurationMs = 2000,
            ActionText = "Cancel",
            ActionCommand = CancelSendCommand,
            ActionCommandParameter = id
        });

        _ = Task.Run((Func<Task?>)(async () =>
        {
            try
            {
                _notifications.Update(new NotificationRequest { Id = id, IsInProgress = true, Progress = -1, Message = "Establishing connection...", DurationMs = 2000 });
                await Task.Delay(2000, token);

                for (var i = 0; i <= 100; i += 5)
                {
                    token.ThrowIfCancellationRequested();

                    while (_sendPaused)
                    {
                        await Task.Delay(100, token);
                    }

                    _notifications.Update(new NotificationRequest { Id = id, IsInProgress = true, Progress = i, Message = $"Sending file... {i}%", DurationMs = 2000, ActionText = "Cancel", ActionCommand = CancelSendCommand, ActionCommandParameter = id });
                    await Task.Delay(150, token);
                }

                _notifications.Update(new NotificationRequest { Id = id, IsInProgress = false, Progress = 100, Level = NotificationLevel.Success, Message = "File sent successfully!", DurationMs = 2000, ActionText = "" });
            }
            catch (OperationCanceledException)
            {
                _notifications.Update(new NotificationRequest
                {
                    Id = id,
                    IsInProgress = false,
                    Level = NotificationLevel.Warning,
                    Message = "Send canceled.",
                    DurationMs = 2000,
                    ActionText = ""
                });
            }
        }));
    }

    [RelayCommand]
    private async Task CancelSendAsync(object? parameter)
    {
        if (_sendCts is null)
        {
            return;
        }

        _sendPaused = true;

        var shouldCancel = await _dialogs.ConfirmAsync(
            "Cancel send",
            "Do you want to cancel sending the file?",
            confirmText: "Yes",
            cancelText: "No");

        if (shouldCancel)
        {
            _sendCts.Cancel();
        }
        else
        {
            _sendPaused = false;
        }
    }

    [RelayCommand]
    private void ShowPersistentNoConnection()
    {
        _persistent ??= _notifications.ShowPersistent(
            "Connection not found. Retrying...",
            level: NotificationLevel.Warning,
            isClosable: false);
    }

    [RelayCommand]
    private void DismissPersistent()
    {
        if (_persistent is not null)
        {
            _notifications.Dismiss(_persistent.Id);
        }
        _persistent = null;
        _notifications.Success("Connection restored!");
    }
}
