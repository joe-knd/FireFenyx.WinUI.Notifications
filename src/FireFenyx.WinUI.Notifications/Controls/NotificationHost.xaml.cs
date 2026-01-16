using FireFenyx.WinUI.Notifications.Extensions;
using FireFenyx.WinUI.Notifications.Models;
using FireFenyx.WinUI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FireFenyx.WinUI.Notifications.Controls;

/// <summary>
/// A UI host control responsible for displaying queued toast-like notifications.
/// </summary>
public sealed partial class NotificationHost : UserControl
{
    private readonly SemaphoreSlim _transitionGate = new(1, 1);
    private CancellationTokenSource? _dismissCts;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHost"/> control.
    /// </summary>
    public NotificationHost()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Displays a notification request within this host.
    /// </summary>
    /// <param name="request">The notification request to display.</param>
    /// <returns>A task that completes when the notification has been shown and (optionally) dismissed.</returns>
    public async Task ShowAsync(NotificationRequest request)
    {
        CancellationTokenSource dismissCts;

        await _transitionGate.WaitAsync().ConfigureAwait(true);
        try
        {
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = new CancellationTokenSource();
            dismissCts = _dismissCts;

            ApplyLevel(request.Level);
            ApplyMaterial(request.Material);

            MessageText.Text = request.Message;

            if (request.IsInProgress)
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = request.Progress < 0;
                if (request.Progress >= 0)
                    ProgressBar.Value = request.Progress;
            }
            else
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }

            ToastBar.CloseButtonClick -= OnClose;
            ToastBar.CloseButtonClick += OnClose;

            await AnimateIn(request.Transition);
        }
        finally
        {
            _transitionGate.Release();
        }

        if (request.DurationMs > 0)
        {
            try
            {
                await Task.Delay(request.DurationMs, dismissCts.Token);
            }
            catch (OperationCanceledException)
            {
                // dismissed manually or replaced by a new notification
            }

            await _transitionGate.WaitAsync().ConfigureAwait(true);
            try
            {
                if (!dismissCts.IsCancellationRequested)
                {
                    await AnimateOut(request.Transition);
                }
            }
            finally
            {
                _transitionGate.Release();
            }
        }
    }

    /// <summary>
    /// Handles the InfoBar close button click.
    /// </summary>
    /// <param name="sender">The InfoBar that raised the event.</param>
    /// <param name="args">Event arguments.</param>
    private void OnClose(InfoBar sender, object args)
    {
        // Cancel any pending auto-dismiss and run one serialized dismiss animation.
        _dismissCts?.Cancel();
        _ = DismissAsync();
    }

    /// <summary>
    /// Dismisses the current notification with an exit animation.
    /// </summary>
    /// <returns>A task that completes once the dismissal animation finishes.</returns>
    private async Task DismissAsync()
    {
        await _transitionGate.WaitAsync().ConfigureAwait(true);
        try
        {
            await AnimateOut(NotificationTransition.SlideAndFade);
        }
        finally
        {
            _transitionGate.Release();
        }
    }

    /// <summary>
    /// Applies visual severity based on the requested notification level.
    /// </summary>
    /// <param name="level">The notification level.</param>
    private void ApplyLevel(NotificationLevel level)
    {
        ToastBar.Severity = level switch
        {
            NotificationLevel.Success => InfoBarSeverity.Success,
            NotificationLevel.Info => InfoBarSeverity.Informational,
            NotificationLevel.Warning => InfoBarSeverity.Warning,
            NotificationLevel.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
    }

    /// <summary>
    /// Applies the requested background material (solid, acrylic, mica) to the toast.
    /// </summary>
    /// <param name="material">The desired material.</param>
    private void ApplyMaterial(NotificationMaterial material)
    {
        var key = material switch
        {
            NotificationMaterial.Acrylic => "NotificationAcrylicBrush",
            NotificationMaterial.Mica => "NotificationMicaBrush",
            _ => "NotificationSolidBrush"
        };

        if (Application.Current.Resources.TryGetValue(key, out var brush))
            ToastBar.Background = (Brush)brush;
    }

    /// <summary>
    /// Plays the entry animation for the toast.
    /// </summary>
    /// <param name="transition">The transition to use.</param>
    /// <returns>A task representing the asynchronous animation.</returns>
    private async Task AnimateIn(NotificationTransition transition)
    {
        Overlay.IsHitTestVisible = true;

        // If the user closed the InfoBar previously, it will remain closed unless reopened.
        ToastBar.IsOpen = true;

        switch (transition)
        {
            case NotificationTransition.Fade:
                await ToastContainer.Fade(1, 250);
                break;

            case NotificationTransition.Scale:
                await ToastContainer.Scale(1.0, 250);
                break;

            case NotificationTransition.SlideAndFade:
                await Task.WhenAll(
                    ToastContainer.RenderTransform.AnimateY(0, 300),
                    ToastContainer.Fade(1, 300));
                break;

            default: // SlideUp
                ToastContainer.Opacity = 1;
                await ToastContainer.RenderTransform.AnimateY(0, 300);
                break;
        }
    }

    /// <summary>
    /// Plays the exit animation for the toast and disables the overlay.
    /// </summary>
    /// <param name="transition">The transition to use.</param>
    /// <returns>A task representing the asynchronous animation.</returns>
    private async Task AnimateOut(NotificationTransition transition)
    {
        // Ensure the InfoBar is closed when we animate out, regardless of whether the user
        // clicked the close button or we timed out.
        ToastBar.IsOpen = false;

        switch (transition)
        {
            case NotificationTransition.Fade:
                await ToastContainer.Fade(0, 200);
                break;

            case NotificationTransition.Scale:
                await ToastContainer.Scale(0.8, 200);
                await ToastContainer.Fade(0, 200);
                break;

            case NotificationTransition.SlideAndFade:
                await Task.WhenAll(
                    ToastContainer.RenderTransform.AnimateY(40, 250),
                    ToastContainer.Fade(0, 250));
                break;

            default: // SlideUp reverse
                await ToastContainer.RenderTransform.AnimateY(40, 250);
                ToastContainer.Opacity = 0;
                break;
        }

        Overlay.IsHitTestVisible = false;
    }

}
