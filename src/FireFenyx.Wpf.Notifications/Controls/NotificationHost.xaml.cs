using FireFenyx.Wpf.Notifications.Extensions;
using FireFenyx.Notifications.Models;
using FireFenyx.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace FireFenyx.Wpf.Notifications.Controls;

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
            new PropertyMetadata(NotificationHostPosition.Bottom, OnLayoutPropertyChanged));

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
            new PropertyMetadata(HorizontalAlignment.Center, OnLayoutPropertyChanged));

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
            new PropertyMetadata(8d, OnLayoutPropertyChanged));

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
            new PropertyMetadata(new Thickness(0, 0, 0, 0), OnLayoutPropertyChanged));

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
            new PropertyMetadata(NotificationBarStyle.AccentStrip));

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

    private readonly SemaphoreSlim _transitionGate = new(1, 1);

    private sealed class NotificationVisual
    {
        public NotificationVisual(Guid id, Grid container, Border bar, Border accentRect,
            TextBlock iconText, TextBlock messageText, ProgressBar progressBar,
            Button actionButton, Button closeButton)
        {
            Id = id;
            Container = container;
            Bar = bar;
            AccentRect = accentRect;
            IconText = iconText;
            MessageText = messageText;
            ProgressBar = progressBar;
            ActionButton = actionButton;
            CloseButton = closeButton;
        }

        public Guid Id { get; }
        public Grid Container { get; }
        public Border Bar { get; }
        public Border AccentRect { get; }
        public TextBlock IconText { get; }
        public TextBlock MessageText { get; }
        public ProgressBar ProgressBar { get; }
        public Button ActionButton { get; }
        public Button CloseButton { get; }
        public CancellationTokenSource? DismissCts { get; set; }
        public Action? Action { get; set; }
        public Func<Task>? ActionAsync { get; set; }
        public string? ActionText { get; set; }
        public ICommand? ActionCommand { get; set; }
        public object? ActionCommandParameter { get; set; }
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
        EnsureThemeResources();
        ApplyLayout();
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

    private static bool _resourcesLoaded;

    private static void EnsureThemeResources()
    {
        if (_resourcesLoaded || Application.Current is null)
            return;

        _resourcesLoaded = true;
        var dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/FireFenyx.Wpf.Notifications;component/Themes/Generic.xaml")
        };
        Application.Current.Resources.MergedDictionaries.Add(dict);
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationHost host)
        {
            host.ApplyLayout();
        }
    }

    private void ApplyLayout()
    {
        if (Stack is null) return;

        Stack.HorizontalAlignment = HostHorizontalAlignment;
        Stack.VerticalAlignment = HostPosition == NotificationHostPosition.Top
            ? VerticalAlignment.Top
            : VerticalAlignment.Bottom;

        var baseMargin = HostPosition == NotificationHostPosition.Top
            ? new Thickness(0, 24, 0, 0)
            : new Thickness(0, 0, 0, 24);

        Stack.Margin = new Thickness(
            baseMargin.Left + HostPadding.Left,
            baseMargin.Top + HostPadding.Top,
            baseMargin.Right + HostPadding.Right,
            baseMargin.Bottom + HostPadding.Bottom);
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
            RenderTransformOrigin = new Point(0.5, 1),
            RenderTransform = new TranslateTransform { Y = initialOffset },
            Margin = new Thickness(0, 0, 0, HostSpacing)
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
            Effect = new DropShadowEffect
            {
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.3,
                Color = Colors.Black
            },
            SnapsToDevicePixels = true,
            ClipToBounds = true
        };

        if (NotificationWidth > 0)
            border.Width = NotificationWidth;
        if (NotificationMaxWidth > 0)
            border.MaxWidth = NotificationMaxWidth;

        // Main grid: accent strip | content | close button
        var mainGrid = new Grid();
        var accentWidth = BarStyle == NotificationBarStyle.Fluent ? 0 : 4;
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(accentWidth) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Accent strip (severity color indicator)
        var accentRect = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1))
        };
        Grid.SetColumn(accentRect, 0);
        mainGrid.Children.Add(accentRect);

        // Content area
        var contentPanel = new StackPanel { Margin = new Thickness(8, 8, 4, 8) };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var iconText = new TextBlock
        {
            Text = "\u2139",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1))
        };
        Grid.SetColumn(iconText, 0);
        headerGrid.Children.Add(iconText);

        var foregroundBrush = Application.Current?.TryFindResource("NotificationForegroundBrush") as Brush
            ?? Brushes.White;

        var messageText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = foregroundBrush
        };
        Grid.SetColumn(messageText, 1);
        headerGrid.Children.Add(messageText);

        var actionButton = new Button
        {
            Visibility = Visibility.Collapsed,
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 80,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        if (Application.Current?.TryFindResource("NotificationActionButtonStyle") is Style actionStyle)
            actionButton.Style = actionStyle;
        Grid.SetColumn(actionButton, 2);
        headerGrid.Children.Add(actionButton);

        contentPanel.Children.Add(headerGrid);

        var progressBar = new ProgressBar
        {
            Height = 4,
            Margin = new Thickness(0, 8, 0, 0),
            Visibility = Visibility.Collapsed,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // Bind the ProgressBar width to the header row so it follows content
        // width instead of inflating the notification via its template's
        // desired size (particularly with the Fluent theme).
        progressBar.SetBinding(WidthProperty,
            new Binding(nameof(ActualWidth)) { Source = headerGrid });

        contentPanel.Children.Add(progressBar);

        Grid.SetColumn(contentPanel, 1);
        mainGrid.Children.Add(contentPanel);

        // Close button
        var closeButton = new Button
        {
            Content = "\u2715",
            Margin = new Thickness(0, 4, 4, 0),
            Foreground = foregroundBrush,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        if (Application.Current?.TryFindResource("NotificationCloseButtonStyle") is Style closeStyle)
            closeButton.Style = closeStyle;
        closeButton.Command = new NotificationActionCommand(_ => CloseClicked(id));
        Grid.SetColumn(closeButton, 2);
        mainGrid.Children.Add(closeButton);

        border.Child = mainGrid;
        container.Children.Add(border);

        return new NotificationVisual(id, container, border, accentRect, iconText, messageText, progressBar, actionButton, closeButton);
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
            visual.Bar.ClearValue(MaxWidthProperty);
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

        visual.CloseButton.Visibility = request.IsClosable ? Visibility.Visible : Visibility.Collapsed;
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
        var (icon, accentBrushKey) = level switch
        {
            NotificationLevel.Success => ("\u2714", "NotificationSuccessBrush"),
            NotificationLevel.Warning => ("\u26A0", "NotificationWarningBrush"),
            NotificationLevel.Error => ("\u2716", "NotificationErrorBrush"),
            _ => ("\u2139", "NotificationInfoBrush")
        };

        var accentBrush = Application.Current?.TryFindResource(accentBrushKey) as Brush
            ?? new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1));

        visual.IconText.Text = icon;
        visual.IconText.Foreground = accentBrush;

        if (BarStyle == NotificationBarStyle.Fluent)
        {
            var filledBrushKey = level switch
            {
                NotificationLevel.Success => "NotificationSuccessFilledBrush",
                NotificationLevel.Warning => "NotificationWarningFilledBrush",
                NotificationLevel.Error => "NotificationErrorFilledBrush",
                _ => "NotificationInfoFilledBrush"
            };

            var filledBrush = Application.Current?.TryFindResource(filledBrushKey) as Brush
                ?? accentBrush;

            visual.Bar.Background = filledBrush;
            visual.AccentRect.Background = Brushes.Transparent;
        }
        else
        {
            visual.AccentRect.Background = accentBrush;
        }
    }

    private void ApplyMaterial(NotificationVisual visual, NotificationMaterial material)
    {
        // In Fluent mode, the severity color IS the background.
        if (BarStyle == NotificationBarStyle.Fluent)
            return;

        var key = material switch
        {
            NotificationMaterial.Acrylic => "NotificationAcrylicBrush",
            NotificationMaterial.Mica => "NotificationMicaBrush",
            _ => "NotificationSolidBrush"
        };

        if (Application.Current?.TryFindResource(key) is Brush brush)
        {
            visual.Bar.Background = brush;
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
    }
}
