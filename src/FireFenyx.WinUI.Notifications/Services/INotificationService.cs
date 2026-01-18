using FireFenyx.WinUI.Notifications.Models;
using System;

namespace FireFenyx.WinUI.Notifications.Services;

/// <summary>
/// Provides a high-level API for showing and updating in-app notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Enqueues a notification request to be displayed by a <see cref="Controls.NotificationHost"/>.
    /// </summary>
    /// <param name="request">The request to display.</param>
    void Show(NotificationRequest request);

    /// <summary>
    /// Shows a progress notification and returns a handle that can be used to update it.
    /// </summary>
    /// <param name="message">The initial message.</param>
    /// <param name="durationMs">The auto-dismiss duration in milliseconds. Each update resets the timer.</param>
    /// <param name="progress">The initial progress value, or -1 for indeterminate.</param>
    /// <returns>A handle that can be used to report progress updates.</returns>
    IProgressNotification ShowProgress(string message, int durationMs = 3000, double progress = -1);

    /// <summary>
    /// Updates an existing notification by <see cref="NotificationRequest.Id"/>.
    /// </summary>
    /// <param name="request">The update request. The <see cref="NotificationRequest.Id"/> must match an existing notification.</param>
    void Update(NotificationRequest request);

    /// <summary>
    /// Shows an informational notification.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="durationMs">The auto-dismiss duration in milliseconds.</param>
    void Info(string message, int durationMs = 3000);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="durationMs">The auto-dismiss duration in milliseconds.</param>
    void Success(string message, int durationMs = 3000);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="durationMs">The auto-dismiss duration in milliseconds.</param>
    void Warning(string message, int durationMs = 3000);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="durationMs">The auto-dismiss duration in milliseconds.</param>
    void Error(string message, int durationMs = 3000);
}

/// <summary>
/// Represents an updatable progress notification.
/// </summary>
public interface IProgressNotification
{
    /// <summary>
    /// Gets the notification identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Reports new progress and optionally updates the message.
    /// </summary>
    /// <param name="progress">The progress value (0-100). Use -1 for indeterminate.</param>
    /// <param name="message">An optional message update.</param>
    void Report(double progress, string? message = null);

    /// <summary>
    /// Marks the progress notification as complete.
    /// </summary>
    /// <param name="message">An optional completion message.</param>
    void Complete(string? message = null);
}
