using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace FureFenyx.WinUI.Notifications.SampleApp.Services;

public sealed class ContentDialogService : IDialogService
{
    private readonly Func<Window?> _windowAccessor;

    public ContentDialogService(Func<Window?> windowAccessor)
    {
        _windowAccessor = windowAccessor;
    }

    public async Task<bool> ConfirmAsync(string title, string message, string confirmText = "Yes", string cancelText = "No")
    {
        var window = _windowAccessor();
        if (window?.Content is not FrameworkElement root)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmText,
            CloseButtonText = cancelText,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = root.XamlRoot
        };

        var result = await dialog.ShowAsync().AsTask().ConfigureAwait(true);
        return result == ContentDialogResult.Primary;
    }
}
