using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Services;

public interface INotificationService
{
    void Show(NotificationRequest request);
    void Info(string message, int durationMs = 3000);
    void Success(string message, int durationMs = 3000);
    void Warning(string message, int durationMs = 3000);
    void Error(string message, int durationMs = 3000);
}
