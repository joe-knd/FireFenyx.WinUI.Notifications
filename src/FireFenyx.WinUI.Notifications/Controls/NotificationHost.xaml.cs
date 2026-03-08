using FireFenyx.WinUI.Notifications.Extensions;
using FireFenyx.Notifications.Models;
using FireFenyx.Notifications.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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

    /// <summary>
    /// Identifies the <see cref="DefaultDurationMs"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DefaultDurationMsProperty =
        DependencyProperty.Register(
            nameof(DefaultDurationMs),
            typeof(int),
            typeof(NotificationHost),
            new PropertyMetadata(3000));

    /// <summary>
    /// Gets or sets the default duration (ms) used when a request does not specify a duration.
    /// </summary>
    public int DefaultDurationMs
    {
        get => (int)GetValue(DefaultDurationMsProperty);
        set => SetValue(DefaultDurationMsProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="DefaultTransition"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DefaultTransitionProperty =
        DependencyProperty.Register(
            nameof(DefaultTransition),
            typeof(NotificationTransition),
            typeof(NotificationHost),
            new PropertyMetadata(NotificationTransition.SlideAndFade));

    /// <summary>
    /// Gets or sets the default transition used when a request does not specify one.
    /// </summary>
    public NotificationTransition DefaultTransition
    {
        get => (NotificationTransition)GetValue(DefaultTransitionProperty);
        set => SetValue(DefaultTransitionProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="DefaultMaterial"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DefaultMaterialProperty =
        DependencyProperty.Register(
            nameof(DefaultMaterial),
            typeof(NotificationMaterial),
            typeof(NotificationHost),
            new PropertyMetadata(NotificationMaterial.Acrylic));

    /// <summary>
    /// Gets or sets the default material used when a request does not specify one.
    /// </summary>
    public NotificationMaterial DefaultMaterial
    {
        get => (NotificationMaterial)GetValue(DefaultMaterialProperty);
        set => SetValue(DefaultMaterialProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="HostHorizontalAlignment"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HostHorizontalAlignmentProperty =
        DependencyProperty.Register(
            nameof(HostHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(NotificationHost),
            new PropertyMetadata(HorizontalAlignment.Center, OnHostLayoutChanged));

    /// <summary>
    /// Gets or sets the horizontal alignment for the notification stack.
    /// </summary>
    public HorizontalAlignment HostHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(HostHorizontalAlignmentProperty);
        set => SetValue(HostHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="HostSpacing"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HostSpacingProperty =
        DependencyProperty.Register(
            nameof(HostSpacing),
            typeof(double),
            typeof(NotificationHost),
            new PropertyMetadata(8d, OnHostLayoutChanged));

    /// <summary>
    /// Gets or sets the spacing between notifications.
    /// </summary>
    public double HostSpacing
    {
        get => (double)GetValue(HostSpacingProperty);
        set => SetValue(HostSpacingProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="HostPadding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HostPaddingProperty =
        DependencyProperty.Register(
            nameof(HostPadding),
            typeof(Thickness),
            typeof(NotificationHost),
            new PropertyMetadata(new Thickness(0, 0, 0, 0), OnHostLayoutChanged));

    /// <summary>
    /// Gets or sets an additional padding applied to the host margin.
    /// </summary>
    public Thickness HostPadding
    {
        get => (Thickness)GetValue(HostPaddingProperty);
        set => SetValue(HostPaddingProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="NotificationWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NotificationWidthProperty =
        DependencyProperty.Register(
            nameof(NotificationWidth),
            typeof(double),
            typeof(NotificationHost),
            new PropertyMetadata(0d));

    /// <summary>
    /// Gets or sets the width used for notification bars.
    /// </summary>
    public double NotificationWidth
    {
        get => (double)GetValue(NotificationWidthProperty);
        set => SetValue(NotificationWidthProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="NotificationMaxWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NotificationMaxWidthProperty =
        DependencyProperty.Register(
            nameof(NotificationMaxWidth),
            typeof(double),
            typeof(NotificationHost),
            new PropertyMetadata(0d));

    /// <summary>
    /// Gets or sets the default maximum width used when a request does not specify a max width.
    /// Set to 0 to disable.
    /// </summary>
    public double NotificationMaxWidth
    {
        get => (double)GetValue(NotificationMaxWidthProperty);
        set => SetValue(NotificationMaxWidthProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="BarStyle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BarStyleProperty =
        DependencyProperty.Register(
            nameof(BarStyle),
            typeof(NotificationBarStyle),
            typeof(NotificationHost),
            new PropertyMetadata(NotificationBarStyle.Fluent));

    /// <summary>
    /// Gets or sets the visual style used for notification bars.
    /// <see cref="NotificationBarStyle.AccentStrip"/> shows a colored strip on the leading edge;
    /// <see cref="NotificationBarStyle.Fluent"/> uses a severity-colored background.
    /// </summary>
    public NotificationBarStyle BarStyle
    {
        get => (NotificationBarStyle)GetValue(BarStyleProperty);
        set => SetValue(BarStyleProperty, value);
    }

    internal VerticalAlignment HostVerticalAlignment
        => HostPosition == NotificationHostPosition.Top ? VerticalAlignment.Top : VerticalAlignment.Bottom;

    internal Thickness HostMargin
        => AddThickness(
            HostPosition == NotificationHostPosition.Top ? new Thickness(0, 24, 0, 0) : new Thickness(0, 0, 0, 24),
            HostPadding);

    private readonly SemaphoreSlim _transitionGate = new(1, 1);
    private sealed class NotificationVisual
    {
        public NotificationVisual(Guid id, Grid container, InfoBar bar, TextBlock messageText, ProgressBar progressBar, Button actionButton)
        {
            Id = id;
            Container = container;
            Bar = bar;
            MessageText = messageText;
            ProgressBar = progressBar;
            ActionButton = actionButton;
        }

        public Guid Id { get; }
        public Grid Container { get; }
        public InfoBar Bar { get; }
        public TextBlock MessageText { get; }
        public ProgressBar ProgressBar { get; }
        public Button ActionButton { get; }
        public CancellationTokenSource? DismissCts { get; set; }
        public Action? Action { get; set; }
        public Func<Task>? ActionAsync { get; set; }
        public string? ActionText { get; set; }
        public ICommand? ActionCommand { get; set; }
        public object? ActionCommandParameter { get; set; }
        public Border? AccentRect { get; set; }
    }

    private readonly Dictionary<Guid, NotificationVisual> _visuals = new();

    private sealed class NotificationActionCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }

    private static ICommand CreateCallbackCommand(Func<Task>? asyncAction, Action? action)
    {
        if (asyncAction is not null)
        {
            var captured = asyncAction;
            return new NotificationActionCommand(_ => _ = RunSafeAsync(captured));
        }

        var capturedAction = action!;
        return new NotificationActionCommand(_ =>
        {
            try { capturedAction(); }
            catch { /* Intentionally ignore exceptions from consumer code. */ }
        });
    }

    private static async Task RunSafeAsync(Func<Task> action)
    {
        try { await action().ConfigureAwait(true); }
        catch { /* Intentionally ignore exceptions from consumer code. */ }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationHost"/> control.
    /// </summary>
    public NotificationHost()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Auto-connect to the default queue registered via AddNotificationServices()
        // if no queue was explicitly set through the attached property.
        if (Services.NotificationHostService.GetQueue(this) is null
            && Services.NotificationDefaults.Queue is { } queue)
        {
            Services.NotificationHostService.SetQueue(this, queue);
        }
    }

    private static void OnHostPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationHost host)
        {
            // Force x:Bind reevaluation.
            host.Bindings.Update();
        }
    }

    private static void OnHostLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationHost host)
        {
            host.Bindings.Update();
        }
    }

    private static Thickness AddThickness(Thickness a, Thickness b)
        => new(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);

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
            if (request.IsUpdate && request.DismissRequested)
            {
                if (request.Id == Guid.Empty)
                {
                    foreach (var id in new List<Guid>(_visuals.Keys))
                    {
                        if (_visuals.TryGetValue(id, out var dv))
                        {
                            dv.DismissCts?.Cancel();
                            dv.DismissCts?.Dispose();
                            dv.DismissCts = null;
                            await AnimateOut(dv, request.Transition);
                            Stack.Children.Remove(dv.Container);
                            _visuals.Remove(id);
                        }
                    }

                    Overlay.IsHitTestVisible = _visuals.Count > 0;
                    return;
                }

                if (_visuals.TryGetValue(request.Id, out var dismissVisual))
                {
                    dismissVisual.DismissCts?.Cancel();
                    dismissVisual.DismissCts?.Dispose();
                    dismissVisual.DismissCts = null;

                    await AnimateOut(dismissVisual, request.Transition);

                    Stack.Children.Remove(dismissVisual.Container);
                    _visuals.Remove(request.Id);
                    Overlay.IsHitTestVisible = _visuals.Count > 0;
                }
                return;
            }

            // Apply per-host defaults when values are not explicitly set.
            var effectiveTransition = request.Transition;
            var effectiveMaterial = request.Material;
            var effectiveDuration = request.DurationMs;

            if (!request.IsUpdate)
            {
                if (effectiveDuration == 3000 && DefaultDurationMs != 3000)
                {
                    effectiveDuration = DefaultDurationMs;
                }
            }

            var effectiveRequest = request;
            if (effectiveTransition != request.Transition || effectiveMaterial != request.Material || effectiveDuration != request.DurationMs)
            {
                effectiveRequest = new NotificationRequest
                {
                    Id = request.Id,
                    Message = request.Message,
                    Level = request.Level,
                    DurationMs = effectiveDuration,
                    IsClosable = request.IsClosable,
                    DismissRequested = request.DismissRequested,
                    ActionText = request.ActionText,
                    Action = request.Action,
                    ActionCommand = request.ActionCommand,
                    ActionAsync = request.ActionAsync,
                    ActionCommandParameter = request.ActionCommandParameter,
                    IsInProgress = request.IsInProgress,
                    Progress = request.Progress,
                    Transition = effectiveTransition,
                    Material = effectiveMaterial,
                    MaxWidth = request.MaxWidth,
                    IsUpdate = request.IsUpdate
                };
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

                // If the first request for a given Id is an update, we still need to show the visual.
                if (effectiveRequest.IsUpdate)
                {
                    ApplyRequestToVisual(visual, effectiveRequest);
                    await AnimateIn(visual, effectiveRequest.Transition);
                    RestartDismissTimer(visual, effectiveRequest);
                    return;
                }
            }

            ApplyRequestToVisual(visual, effectiveRequest);

            if (!effectiveRequest.IsUpdate)
            {
                await AnimateIn(visual, effectiveRequest.Transition);
            }

            RestartDismissTimer(visual, effectiveRequest);
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
            Width = NotificationWidth > 0 ? NotificationWidth : double.NaN,
            CornerRadius = new CornerRadius(6),
            IsHitTestVisible = true
        };

        if (NotificationMaxWidth > 0)
        {
            bar.MaxWidth = NotificationMaxWidth;
        }

        var message = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var progress = new ProgressBar
        {
            Height = 4,
            Margin = new Thickness(0, 8, 0, 0),
            Visibility = Visibility.Collapsed,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        var actionButton = new Button
        {
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 80
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(message, 0);
        message.VerticalAlignment = VerticalAlignment.Center;
        headerGrid.Children.Add(message);

        Grid.SetColumn(actionButton, 1);
        actionButton.Margin = new Thickness(12, 4, 0, 0);
        actionButton.VerticalAlignment = VerticalAlignment.Center;
        headerGrid.Children.Add(actionButton);

        var contentStack = new StackPanel();
        contentStack.Children.Add(headerGrid);
        contentStack.Children.Add(progress);
        bar.Content = contentStack;

        bar.CloseButtonClick += (_, __) => CloseClicked(id);

        container.Children.Add(bar);

        var visual = new NotificationVisual(id, container, bar, message, progress, actionButton);

        if (BarStyle == NotificationBarStyle.AccentStrip)
        {
            var accentRect = new Border
            {
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(6, 0, 0, 6)
            };
            container.Children.Add(accentRect);
            visual.AccentRect = accentRect;
        }

        return visual;
    }

    private void CloseClicked(Guid id)
        => _ = DismissAsync(id, NotificationTransition.SlideAndFade);

    private void ApplyRequestToVisual(NotificationVisual visual, NotificationRequest request)
    {
        Overlay.IsHitTestVisible = _visuals.Count > 0;

        ApplyLevel(visual, request.Level);
        ApplyMaterial(visual, request.Material);

        if (request.MaxWidth is double maxWidth)
        {
            visual.Bar.MaxWidth = maxWidth;
        }
        else if (NotificationMaxWidth > 0)
        {
            visual.Bar.MaxWidth = NotificationMaxWidth;
        }
        else
        {
            visual.Bar.ClearValue(FrameworkElement.MaxWidthProperty);
        }

        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            visual.MessageText.Text = request.Message;
        }

        if (request.IsInProgress)
        {
            if (visual.ProgressBar.Visibility != Visibility.Visible)
            {
                visual.ProgressBar.Opacity = 0;
                visual.ProgressBar.Visibility = Visibility.Visible;
                _ = visual.ProgressBar.Fade(1, 150);
            }

            visual.ProgressBar.IsIndeterminate = request.Progress < 0;
            if (request.Progress >= 0)
            {
                visual.ProgressBar.Value = request.Progress;
            }
        }
        else
        {
            if (visual.ProgressBar.Visibility == Visibility.Visible)
            {
                _ = HideWithFadeAsync(visual.ProgressBar);
            }
        }

        // Sticky action: if an update doesn't specify action fields, keep the previous action configuration.
        var hasActionUpdate = request.ActionText is not null || request.ActionCommand is not null || request.ActionAsync is not null || request.Action is not null || request.ActionCommandParameter is not null;
        if (!request.IsUpdate || hasActionUpdate)
        {
            if (!string.IsNullOrWhiteSpace(request.ActionText) && (request.ActionCommand is not null || request.ActionAsync is not null || request.Action is not null))
            {
                visual.ActionText = request.ActionText;
                visual.Action = request.Action;
                visual.ActionAsync = request.ActionAsync;
                visual.ActionCommand = request.ActionCommand;
                visual.ActionCommandParameter = request.ActionCommandParameter;
            }
            else
            {
                visual.ActionText = null;
                visual.Action = null;
                visual.ActionAsync = null;
                visual.ActionCommand = null;
                visual.ActionCommandParameter = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(visual.ActionText) && (visual.ActionCommand is not null || visual.ActionAsync is not null || visual.Action is not null))
        {
            visual.ActionButton.Content = visual.ActionText;
            if (visual.ActionCommand is not null)
            {
                visual.ActionButton.Command = visual.ActionCommand;
                visual.ActionButton.CommandParameter = visual.ActionCommandParameter;
            }
            else
            {
                visual.ActionButton.Command = CreateCallbackCommand(visual.ActionAsync, visual.Action);
                visual.ActionButton.CommandParameter = null;
            }
            if (visual.ActionButton.Visibility != Visibility.Visible)
            {
                visual.ActionButton.Opacity = 0;
                visual.ActionButton.Visibility = Visibility.Visible;
                _ = visual.ActionButton.Fade(1, 150);
            }
        }
        else
        {
            visual.ActionButton.Command = null;
            visual.ActionButton.CommandParameter = null;
            if (visual.ActionButton.Visibility == Visibility.Visible)
            {
                _ = HideWithFadeAsync(visual.ActionButton);
            }
        }

        visual.Bar.IsClosable = request.IsClosable;
        visual.Bar.IsOpen = true;
    }

    private static async Task HideWithFadeAsync(UIElement element)
    {
        try
        {
            await element.Fade(0, 150).ConfigureAwait(true);
        }
        catch
        {
            // Ignore animation failures.
        }
        finally
        {
            element.Visibility = Visibility.Collapsed;
        }
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

    private void ApplyLevel(NotificationVisual visual, NotificationLevel level)
    {
        visual.Bar.Severity = level switch
        {
            NotificationLevel.Success => InfoBarSeverity.Success,
            NotificationLevel.Info => InfoBarSeverity.Informational,
            NotificationLevel.Warning => InfoBarSeverity.Warning,
            NotificationLevel.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };

        if (BarStyle == NotificationBarStyle.AccentStrip && visual.AccentRect is not null)
        {
            var brushKey = level switch
            {
                NotificationLevel.Success => "NotificationSuccessBrush",
                NotificationLevel.Warning => "NotificationWarningBrush",
                NotificationLevel.Error => "NotificationErrorBrush",
                _ => "NotificationInfoBrush"
            };

            if (Application.Current.Resources.TryGetValue(brushKey, out var brush))
            {
                visual.AccentRect.Background = (Brush)brush;
            }
        }
    }

    private void ApplyMaterial(NotificationVisual visual, NotificationMaterial material)
    {
        // In Fluent mode, let InfoBar's built-in severity styling control the background.
        if (BarStyle == NotificationBarStyle.Fluent)
            return;

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
                await Task.WhenAll(
                    visual.Container.RenderTransform.AnimateY(exitOffset, 250),
                    visual.Container.Fade(0, 250));
                break;
        }

        visual.Bar.IsOpen = false;
    }

}
