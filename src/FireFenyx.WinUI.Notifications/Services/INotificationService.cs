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

    /// <summary>
    /// Dismisses a notification by id.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    void Dismiss(Guid id);

    /// <summary>
    /// Dismisses all notifications currently shown by the host.
    /// </summary>
    void DismissAll();

    /// <summary>
    /// Shows a persistent notification (no auto-dismiss) and returns a handle that can be used to update or dismiss it.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="isClosable">Whether the user can dismiss the notification.</param>
    /// <returns>A handle that can be used to dismiss or update the notification.</returns>
    IPersistentNotification ShowPersistent(string message, NotificationLevel level = NotificationLevel.Info, bool isClosable = false);
}

/// <summary>
/// Represents a persistent notification that can be updated and dismissed programmatically.
/// </summary>
public interface IPersistentNotification
{
    /// <summary>
    /// Gets the notification identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Updates the message and/or level of the notification and optionally sets a new dismiss duration.
    /// </summary>
    /// <param name="message">The new message, or <see langword="null"/> to keep the current message.</param>
    /// <param name="level">The new level, or <see langword="null"/> to keep the current level.</param>
    /// <param name="durationMs">Optional duration to start auto-dismiss. Use <c>0</c> or negative to keep persistent.</param>
    void Update(string? message = null, NotificationLevel? level = null, int? durationMs = null);

    /// <summary>
    /// Dismisses the notification.
    /// </summary>
    void Dismiss();
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
    /// Sets the progress indicator to indeterminate mode and optionally updates the message.
    /// </summary>
    /// <param name="message">An optional message update.</param>
    void Indeterminate(string? message = null);

    /// <summary>
    /// Marks the progress notification as complete.
    /// </summary>
    /// <param name="message">An optional completion message.</param>
    void Complete(string? message = null);
}
