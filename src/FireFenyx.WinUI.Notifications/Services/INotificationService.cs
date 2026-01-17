using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Services;

public interface INotificationService
{
    void Show(NotificationRequest request);

    IProgressNotification ShowProgress(string message, int durationMs = 3000, double progress = -1);
    void Update(NotificationRequest request);
    void Info(string message, int durationMs = 3000);
    void Success(string message, int durationMs = 3000);
    void Warning(string message, int durationMs = 3000);
    void Error(string message, int durationMs = 3000);
}

public interface IProgressNotification
{
    Guid Id { get; }
    void Report(double progress, string? message = null);
    void Complete(string? message = null);
}
