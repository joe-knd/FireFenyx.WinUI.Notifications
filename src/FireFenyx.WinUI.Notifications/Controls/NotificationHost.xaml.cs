using FireFenyx.WinUI.Notifications.Extensions;
using FireFenyx.WinUI.Notifications.Models;
using FireFenyx.WinUI.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
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
    private sealed class NotificationVisual
    {
        public NotificationVisual(Guid id, Grid container, InfoBar bar, TextBlock messageText, Microsoft.UI.Xaml.Controls.ProgressBar progressBar)
        {
            Id = id;
            Container = container;
            Bar = bar;
            MessageText = messageText;
            ProgressBar = progressBar;
        }

        public Guid Id { get; }
        public Grid Container { get; }
        public InfoBar Bar { get; }
        public TextBlock MessageText { get; }
        public Microsoft.UI.Xaml.Controls.ProgressBar ProgressBar { get; }
        public CancellationTokenSource? DismissCts { get; set; }
    }

    private readonly Dictionary<Guid, NotificationVisual> _visuals = new();

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
        await _transitionGate.WaitAsync().ConfigureAwait(true);
        try
        {
            if (!_visuals.TryGetValue(request.Id, out var visual))
            {
                visual = CreateVisual(request.Id);
                _visuals.Add(request.Id, visual);
                Stack.Children.Add(visual.Container);
            }

            ApplyRequestToVisual(visual, request);

            if (!request.IsUpdate)
            {
                await AnimateIn(visual, request.Transition);
            }

            RestartDismissTimer(visual, request);
        }
        finally
        {
            _transitionGate.Release();
        }
    }

    // NOTE: Per-toast close handling is wired in CreateVisual.

    private NotificationVisual CreateVisual(Guid id)
    {
        var container = new Grid
        {
            Opacity = 0,
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 1),
            RenderTransform = new TranslateTransform { Y = 40 }
        };

        var bar = new InfoBar
        {
            IsOpen = true,
            IsClosable = true,
            Width = 420,
            CornerRadius = new CornerRadius(6),
            IsHitTestVisible = true
        };

        var message = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var progress = new Microsoft.UI.Xaml.Controls.ProgressBar
        {
            Height = 4,
            Margin = new Thickness(0, 8, 0, 0),
            Visibility = Visibility.Collapsed
        };

        var stack = new StackPanel();
        stack.Children.Add(message);
        stack.Children.Add(progress);
        bar.Content = stack;

        bar.CloseButtonClick += (_, __) => CloseClicked(id);

        container.Children.Add(bar);

        return new NotificationVisual(id, container, bar, message, progress);
    }

    private void CloseClicked(Guid id)
        => _ = DismissAsync(id, NotificationTransition.SlideAndFade);

    private void ApplyRequestToVisual(NotificationVisual visual, NotificationRequest request)
    {
        Overlay.IsHitTestVisible = _visuals.Count > 0;

        ApplyLevel(visual, request.Level);
        ApplyMaterial(visual, request.Material);

        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            visual.MessageText.Text = request.Message;
        }

        if (request.IsInProgress)
        {
            visual.ProgressBar.Visibility = Visibility.Visible;
            visual.ProgressBar.IsIndeterminate = request.Progress < 0;
            if (request.Progress >= 0)
            {
                visual.ProgressBar.Value = request.Progress;
            }
        }
        else
        {
            visual.ProgressBar.Visibility = Visibility.Collapsed;
        }

        visual.Bar.IsOpen = true;
    }

    private void RestartDismissTimer(NotificationVisual visual, NotificationRequest request)
    {
        if (request.DurationMs <= 0)
        {
            visual.DismissCts?.Cancel();
            return;
        }

        visual.DismissCts?.Cancel();
        visual.DismissCts?.Dispose();
        visual.DismissCts = new CancellationTokenSource();

        var token = visual.DismissCts.Token;
        _ = DismissAfterDelayAsync(visual.Id, request.Transition, request.DurationMs, token);
    }

    private async Task DismissAfterDelayAsync(Guid id, NotificationTransition transition, int durationMs, CancellationToken token)
    {
        try
        {
            await Task.Delay(durationMs, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await DismissAsync(id, transition).ConfigureAwait(true);
    }

    private async Task DismissAsync(Guid id, NotificationTransition transition)
    {
        await _transitionGate.WaitAsync().ConfigureAwait(true);
        try
        {
            if (!_visuals.TryGetValue(id, out var visual))
            {
                return;
            }

            visual.DismissCts?.Cancel();
            visual.DismissCts?.Dispose();
            visual.DismissCts = null;

            await AnimateOut(visual, transition);

            Stack.Children.Remove(visual.Container);
            _visuals.Remove(id);

            Overlay.IsHitTestVisible = _visuals.Count > 0;
        }
        finally
        {
            _transitionGate.Release();
        }
    }

    private static void ApplyLevel(NotificationVisual visual, NotificationLevel level)
    {
        visual.Bar.Severity = level switch
        {
            NotificationLevel.Success => InfoBarSeverity.Success,
            NotificationLevel.Info => InfoBarSeverity.Informational,
            NotificationLevel.Warning => InfoBarSeverity.Warning,
            NotificationLevel.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
    }

    private static void ApplyMaterial(NotificationVisual visual, NotificationMaterial material)
    {
        var key = material switch
        {
            NotificationMaterial.Acrylic => "NotificationAcrylicBrush",
            NotificationMaterial.Mica => "NotificationMicaBrush",
            _ => "NotificationSolidBrush"
        };

        if (Application.Current.Resources.TryGetValue(key, out var brush))
        {
            visual.Bar.Background = (Brush)brush;
        }
    }

    private async Task AnimateIn(NotificationVisual visual, NotificationTransition transition)
    {
        switch (transition)
        {
            case NotificationTransition.Fade:
                await visual.Container.Fade(1, 250);
                break;

            case NotificationTransition.Scale:
                await visual.Container.Scale(1.0, 250);
                break;

            case NotificationTransition.SlideAndFade:
                await Task.WhenAll(
                    visual.Container.RenderTransform.AnimateY(0, 300),
                    visual.Container.Fade(1, 300));
                break;

            default: // SlideUp
                visual.Container.Opacity = 1;
                await visual.Container.RenderTransform.AnimateY(0, 300);
                break;
        }
    }

    private async Task AnimateOut(NotificationVisual visual, NotificationTransition transition)
    {
        visual.Bar.IsOpen = false;

        switch (transition)
        {
            case NotificationTransition.Fade:
                await visual.Container.Fade(0, 200);
                break;

            case NotificationTransition.Scale:
                await visual.Container.Scale(0.8, 200);
                await visual.Container.Fade(0, 200);
                break;

            case NotificationTransition.SlideAndFade:
                await Task.WhenAll(
                    visual.Container.RenderTransform.AnimateY(40, 250),
                    visual.Container.Fade(0, 250));
                break;

            default: // SlideUp reverse
                await visual.Container.RenderTransform.AnimateY(40, 250);
                visual.Container.Opacity = 0;
                break;
        }
    }

}
