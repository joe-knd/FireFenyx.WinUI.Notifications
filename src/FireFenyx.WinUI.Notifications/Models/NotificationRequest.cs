using System;
using System.Collections.Generic;
using System.Text;

namespace FireFenyx.WinUI.Notifications.Models;

public sealed class NotificationRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Message { get; init; } = string.Empty;
    public NotificationLevel Level { get; init; } = NotificationLevel.Info;
    public int DurationMs { get; init; } = 3000;

    public string? ActionText { get; init; }
    public Action? Action { get; init; }

    public bool IsInProgress { get; init; }
    public double Progress { get; init; } = -1; // -1 = indeterminate

    public NotificationTransition Transition { get; init; } = NotificationTransition.SlideAndFade;
    public NotificationMaterial Material { get; init; } = NotificationMaterial.Acrylic;

    /// <summary>
    /// When set, this request updates an existing notification with the same <see cref="Id"/>
    /// rather than creating a new visual.
    /// </summary>
    public bool IsUpdate { get; init; }

}
