using FireFenyx.WinUI.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FireFenyx.WinUI.Notifications.Services;

public interface INotificationQueue
{
    void Enqueue(NotificationRequest request);

    void SetProcessor(Func<NotificationRequest, Task> processor);

}
