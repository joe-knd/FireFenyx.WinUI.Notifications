using System.Threading.Tasks;

namespace FireFenyx.WinUI.Notifications.SampleApp.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string confirmText = "Yes", string cancelText = "No");
}
