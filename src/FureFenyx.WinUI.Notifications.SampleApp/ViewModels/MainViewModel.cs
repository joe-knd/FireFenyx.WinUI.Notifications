using FireFenyx.WinUI.Notifications.Models;
using FireFenyx.WinUI.Notifications.Services;
using FureFenyx.WinUI.Notifications.SampleApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FureFenyx.WinUI.Notifications.SampleApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public INotificationQueue NotificationQueue { get; }
    private readonly INotificationService _notifications;
    private readonly IDialogService _dialogs;
    private IPersistentNotification? _persistent;

    private CancellationTokenSource? _sendCts;
    private volatile bool _sendPaused;

    public MainViewModel()
    {
        NotificationQueue = App.Services.GetRequiredService<INotificationQueue>();
        _notifications = App.Services.GetRequiredService<INotificationService>();
        _dialogs = App.Services.GetRequiredService<IDialogService>();
    }

    public void ShowSuccess()
        => _notifications.Success("Operation completed successfully!");

    public void ShowWarning()
        => _notifications.Warning("This is a warning message.");

    public void ShowError()
        => _notifications.Error("Something went wrong!", durationMs: 5000);

    public void ShowProgress()
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

    public void SendFileComplexScenario()
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

        _ = Task.Run(async () =>
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

                _notifications.Update(new NotificationRequest { Id = id, IsInProgress = false, Progress = 100, Level = NotificationLevel.Success, Message = "File sent successfully!", DurationMs = 2000 });
            }
            catch (OperationCanceledException)
            {
                _notifications.Update(new NotificationRequest
                {
                    Id = id,
                    IsInProgress = false,
                    Level = NotificationLevel.Warning,
                    Message = "Send canceled.",
                    DurationMs = 2000
                });
            }
        });
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

    public void ShowPersistentNoConnection()
    {
        _persistent ??= _notifications.ShowPersistent(
            "Connection not found. Retrying...",
            level: NotificationLevel.Warning,
            isClosable: false);
    }

    public void DismissPersistent()
    {
        if (_persistent is not null)
        {
            _notifications.Dismiss(_persistent.Id);
        }
        _persistent = null;
        _notifications.Success("Connection restored!");
    }

}
