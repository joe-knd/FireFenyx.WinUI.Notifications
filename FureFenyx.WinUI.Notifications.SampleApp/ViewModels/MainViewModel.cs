using FireFenyx.WinUI.Notifications.Models;
using FireFenyx.WinUI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FureFenyx.WinUI.Notifications.SampleApp.ViewModels;

public class MainViewModel
{
    public INotificationQueue NotificationQueue { get; }
    private readonly INotificationService _notifications;

    public MainViewModel()
    {
        NotificationQueue = App.Services.GetRequiredService<INotificationQueue>();
        _notifications = App.Services.GetRequiredService<INotificationService>();
    }

    public void ShowSuccess()
        => _notifications.Success("Operation completed successfully!");

    public void ShowWarning()
        => _notifications.Warning("This is a warning message.");

    public void ShowError()
        => _notifications.Error("Something went wrong!", durationMs: 5000);

    public void ShowProgress()
    {
        _notifications.Show(new NotificationRequest
        {
            Message = "Uploading...",
            Level = NotificationLevel.Info,
            IsInProgress = true,
            Progress = 42
        });
    }

}
