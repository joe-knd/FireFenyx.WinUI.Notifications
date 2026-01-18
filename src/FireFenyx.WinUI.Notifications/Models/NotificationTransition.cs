namespace FireFenyx.WinUI.Notifications.Models;

/// <summary>
/// Defines the animation used when showing and dismissing a notification.
/// </summary>
public enum NotificationTransition
{
    /// <summary>
    /// Slides up from the bottom of the host.
    /// </summary>
    SlideUp,

    /// <summary>
    /// Fades in and out.
    /// </summary>
    Fade,

    /// <summary>
    /// Scales in and out.
    /// </summary>
    Scale,

    /// <summary>
    /// Slides and fades simultaneously.
    /// </summary>
    SlideAndFade
}
