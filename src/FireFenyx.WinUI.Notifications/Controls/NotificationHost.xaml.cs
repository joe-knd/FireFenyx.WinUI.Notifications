using FireFenyx.WinUI.Notifications.Extensions;
using FireFenyx.WinUI.Notifications.Models;
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
    /// <summary>
    /// Defines where notifications should appear within the host.
    /// </summary>
    public enum NotificationHostPosition
    {
        /// <summary>
        /// Notifications are stacked from the bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// Notifications are stacked from the top.
        /// </summary>
        Top
    }

    /// <summary>
    /// Identifies the <see cref="HostPosition"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HostPositionProperty =
        DependencyProperty.Register(
            nameof(HostPosition),
            typeof(NotificationHostPosition),
            typeof(NotificationHost),
            new PropertyMetadata(NotificationHostPosition.Bottom, OnHostPositionChanged));

    /// <summary>
    /// Gets or sets where notifications should appear within the host.
    /// </summary>
    public NotificationHostPosition HostPosition
    {
        get => (NotificationHostPosition)GetValue(HostPositionProperty);
        set => SetValue(HostPositionProperty, value);
    }

    internal VerticalAlignment HostVerticalAlignment
        => HostPosition == NotificationHostPosition.Top ? VerticalAlignment.Top : VerticalAlignment.Bottom;

    internal Thickness HostMargin
        => HostPosition == NotificationHostPosition.Top ? new Thickness(0, 24, 0, 0) : new Thickness(0, 0, 0, 24);

    private readonly SemaphoreSlim _transitionGate = new(1, 1);
    private sealed class NotificationVisual
    {
        public NotificationVisual(Guid id, Grid container, InfoBar bar, TextBlock messageText, ProgressBar progressBar)
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
        public ProgressBar ProgressBar { get; }
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

    private static void OnHostPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationHost host)
        {
            // Force x:Bind reevaluation.
            host.Bindings.Update();
        }
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
            if (request.IsUpdate && request.DurationMs == 1 && string.IsNullOrEmpty(request.Message))
            {
                await DismissAsync(request.Id, request.Transition);
                return;
            }

            if (!_visuals.TryGetValue(request.Id, out var visual))
            {
                visual = CreateVisual(request.Id);
                _visuals.Add(request.Id, visual);
                // When positioned at the top, insert newest notifications at the top of the stack.
                if (HostPosition == NotificationHostPosition.Top)
                {
                    Stack.Children.Insert(0, visual.Container);
                }
                else
                {
                    Stack.Children.Add(visual.Container);
                }
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
        var initialOffset = HostPosition == NotificationHostPosition.Top ? -40 : 40;
        var container = new Grid
        {
            Opacity = 0,
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 1),
            RenderTransform = new TranslateTransform { Y = initialOffset }
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
        var progress = new ProgressBar
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

        visual.Bar.IsClosable = request.IsClosable;
        visual.Bar.IsOpen = true;
    }

    private void RestartDismissTimer(NotificationVisual visual, NotificationRequest request)
    {
        if (request.DurationMs <= 0)
        {
            visual.DismissCts?.Cancel();
            visual.DismissCts?.Dispose();
            visual.DismissCts = null;
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

        var exitOffset = HostPosition == NotificationHostPosition.Top ? -40 : 40;

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
                    visual.Container.RenderTransform.AnimateY(exitOffset, 250),
                    visual.Container.Fade(0, 250));
                break;

            default: // SlideUp reverse
                await visual.Container.RenderTransform.AnimateY(exitOffset, 250);
                visual.Container.Opacity = 0;
                break;
        }
    }

}
