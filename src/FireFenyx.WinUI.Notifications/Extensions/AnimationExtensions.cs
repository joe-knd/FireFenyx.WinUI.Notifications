using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace FireFenyx.WinUI.Notifications.Extensions;

/// <summary>
/// Provides simple animation helpers for WinUI elements.
/// </summary>
public static class AnimationExtensions
{
    /// <summary>
    /// Animates the Y translation of a <see cref="Transform"/>.
    /// </summary>
    /// <param name="transform">The transform to animate.</param>
    /// <param name="to">The target Y value.</param>
    /// <param name="durationMs">Animation duration in milliseconds.</param>
    /// <returns>A task that completes when the animation finishes.</returns>
    public static Task AnimateY(this Transform transform, double to, int durationMs)
    {
        var sb = new Storyboard();
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, "Y");
        sb.Children.Add(anim);

        var tcs = new TaskCompletionSource();
        sb.Completed += (_, _) => tcs.SetResult();
        sb.Begin();
        return tcs.Task;
    }

    /// <summary>
    /// Animates the opacity of a <see cref="UIElement"/>.
    /// </summary>
    /// <param name="element">The element to animate.</param>
    /// <param name="to">The target opacity.</param>
    /// <param name="durationMs">Animation duration in milliseconds.</param>
    /// <returns>A task that completes when the animation finishes.</returns>
    public static Task Fade(this UIElement element, double to, int durationMs)
    {
        var sb = new Storyboard();
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = TimeSpan.FromMilliseconds(durationMs)
        };

        Storyboard.SetTarget(anim, element);
        Storyboard.SetTargetProperty(anim, "Opacity");
        sb.Children.Add(anim);

        var tcs = new TaskCompletionSource();
        sb.Completed += (_, _) => tcs.SetResult();
        sb.Begin();
        return tcs.Task;
    }

    /// <summary>
    /// Animates the scale of a <see cref="UIElement"/>.
    /// </summary>
    /// <param name="element">The element to animate.</param>
    /// <param name="to">The target scale value.</param>
    /// <param name="durationMs">Animation duration in milliseconds.</param>
    /// <returns>A task that completes when the animation finishes.</returns>
    public static Task Scale(this UIElement element, double to, int durationMs)
    {
        var transform = element.RenderTransform as ScaleTransform;
        if (transform is null)
        {
            transform = new ScaleTransform { ScaleX = 0.8, ScaleY = 0.8 };
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 1);
        }

        var sb = new Storyboard();
        var animX = new DoubleAnimation { To = to, Duration = TimeSpan.FromMilliseconds(durationMs) };
        var animY = new DoubleAnimation { To = to, Duration = TimeSpan.FromMilliseconds(durationMs) };

        Storyboard.SetTarget(animX, transform);
        Storyboard.SetTarget(animY, transform);

        Storyboard.SetTargetProperty(animX, "ScaleX");
        Storyboard.SetTargetProperty(animY, "ScaleY");

        sb.Children.Add(animX);
        sb.Children.Add(animY);

        var tcs = new TaskCompletionSource();
        sb.Completed += (_, _) => tcs.SetResult();
        sb.Begin();
        return tcs.Task;
    }

}
