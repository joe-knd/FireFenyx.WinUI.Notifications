using System;

namespace FireFenyx.WinUI.Notifications.Models;

/// <summary>
/// Represents a notification to display (or update) in a <see cref="Controls.NotificationHost"/>.
/// </summary>
public sealed class NotificationRequest
{
    /// <summary>
    /// Gets the identifier for this notification. Updates target an existing notification by id.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the message displayed for the notification.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the severity level.
    /// </summary>
    public NotificationLevel Level { get; init; } = NotificationLevel.Info;

    /// <summary>
    /// Gets the auto-dismiss duration in milliseconds. Specify <c>0</c> or a negative value
    /// to keep the notification visible until updated or dismissed programmatically.
    /// </summary>
    public int DurationMs { get; init; } = 3000;

    /// <summary>
    /// Gets whether the notification can be dismissed by the user.
    /// </summary>
    public bool IsClosable { get; init; } = true;

    /// <summary>
    /// Gets optional action button text.
    /// </summary>
    public string? ActionText { get; init; }

    /// <summary>
    /// Gets an optional callback executed when the action is invoked.
    /// </summary>
    public Action? Action { get; init; }

    /// <summary>
    /// Gets whether this request should show a progress bar.
    /// </summary>
    public bool IsInProgress { get; init; }

    /// <summary>
    /// Gets the progress value. Use -1 for indeterminate.
    /// </summary>
    public double Progress { get; init; } = -1; // -1 = indeterminate

    /// <summary>
    /// Gets the transition used for showing and dismissing the notification.
    /// </summary>
    public NotificationTransition Transition { get; init; } = NotificationTransition.SlideAndFade;

    /// <summary>
    /// Gets the background material.
    /// </summary>
    public NotificationMaterial Material { get; init; } = NotificationMaterial.Acrylic;

    /// <summary>
    /// When set, this request updates an existing notification with the same <see cref="Id"/>
    /// rather than creating a new visual.
    /// </summary>
    public bool IsUpdate { get; init; }

}
