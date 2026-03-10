# FireFenyx.Notifications

Toast-style in-app notification library for **WPF** and **WinUI 3** with animated transitions, progress tracking, persistent notifications, and two built-in visual styles.

[![CI](https://github.com/joe-knd/FireFenyx.Notifications/actions/workflows/ci.yml/badge.svg)](https://github.com/joe-knd/FireFenyx.Notifications/actions/workflows/ci.yml)
[![NuGet WinUI](https://img.shields.io/nuget/v/FireFenyx.WinUI.Notifications.svg?label=WinUI)](https://www.nuget.org/packages/FireFenyx.WinUI.Notifications)
[![NuGet WPF](https://img.shields.io/nuget/v/FireFenyx.Wpf.Notifications.svg?label=WPF)](https://www.nuget.org/packages/FireFenyx.Wpf.Notifications)
[![NuGet Abstractions](https://img.shields.io/nuget/v/FireFenyx.Notifications.Abstractions.svg?label=Abstractions)](https://www.nuget.org/packages/FireFenyx.Notifications.Abstractions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![.NET 8](https://img.shields.io/badge/.NET-8-blue)
![.NET 9](https://img.shields.io/badge/.NET-9-blue)
![.NET 10](https://img.shields.io/badge/.NET-10-purple)

---

## Demo

| WPF | WinUI |
|-----|-------|
| [▶ Watch WPF demo](https://github.com/joe-knd/FireFenyx.Notifications/raw/main/videos/wpf.mp4) | [▶ Watch WinUI demo](https://github.com/joe-knd/FireFenyx.Notifications/raw/main/videos/winUI.mp4) |

## Notification Styles

### Fluent

A severity-colored background inspired by the WinUI InfoBar.

| WPF | WinUI |
|-----|-------|
| ![WPF Fluent](https://raw.githubusercontent.com/joe-knd/FireFenyx.Notifications/main/images/Wpf-Fluent.png) | ![WinUI Fluent](https://raw.githubusercontent.com/joe-knd/FireFenyx.Notifications/main/images/winUI-Fluent.png) |

### AccentStrip

A colored accent strip on the leading edge with a material background.

| WPF | WinUI |
|-----|-------|
| ![WPF Strip](https://raw.githubusercontent.com/joe-knd/FireFenyx.Notifications/main/images/Wpf-Strip.png) | ![WinUI Strip](https://raw.githubusercontent.com/joe-knd/FireFenyx.Notifications/main/images/WinUI-Strip.png) |

---

## Features

- **Success / Info / Warning / Error** — one-liner convenience methods
- **Progress notifications** — determinate and indeterminate with live updates
- **Persistent notifications** — stay visible until dismissed programmatically
- **Action buttons** — attach an `ICommand`, `Action`, or `Func<Task>` to any notification
- **Animated transitions** — SlideUp, Fade, Scale, SlideAndFade
- **Background materials** — Solid, Acrylic, Mica
- **Two bar styles** — Fluent and AccentStrip, switchable at runtime
- **Auto-dismiss** with configurable duration
- **Stacking** — multiple notifications stack with configurable spacing and position (Top / Bottom)
- **DI-friendly** — one call to register, auto-connects to the host control
- **Shared abstractions** — write your ViewModel once, run on both WPF and WinUI

## Solution Structure

```
FireFenyx.Notifications.Abstractions      → Shared models, interfaces, service implementation (net10.0)
FireFenyx.Wpf.Notifications              → WPF control + queue (net10.0-windows)
FireFenyx.WinUI.Notifications            → WinUI 3 control + queue (net10.0-windows10.0.22621.0)
FireFenyx.Notifications.SampleApp.Shared → Shared ViewModel + IDialogService (net10.0)
FireFenyx.Wpf.Notifications.SampleApp    → WPF sample app
FireFenyx.WinUI.Notifications.SampleApp  → WinUI 3 sample app
FireFenyx.Wpf.Notifications.Tests        → WPF unit tests (xUnit)
FireFenyx.WinUI.Notifications.Tests      → WinUI unit tests (xUnit)
```

---

## Installation

Install the package for your target framework:

**WPF**
```
dotnet add package FireFenyx.Wpf.Notifications
```

**WinUI 3**
```
dotnet add package FireFenyx.WinUI.Notifications
```

Each package brings in `FireFenyx.Notifications.Abstractions` automatically.

---

## Getting Started

### 1. Register services

```csharp
var services = new ServiceCollection();
services.AddNotificationServices();
```

### 2. Drop the host control in your window

**WPF**

```xml
<notifications:NotificationHost
    BarStyle="{Binding SelectedBarStyle}"
    HostPosition="Bottom"
    DefaultDurationMs="4000"
    DefaultTransition="SlideAndFade"
    DefaultMaterial="Acrylic"
    HostHorizontalAlignment="Center"
    HostSpacing="10"
    HostPadding="0,0,0,8" />
```

**WinUI**

```xml
<notifications:NotificationHost
    BarStyle="{x:Bind ViewModel.SelectedBarStyle, Mode=OneWay}"
    HostPosition="Bottom"
    DefaultDurationMs="4000"
    DefaultTransition="SlideAndFade"
    DefaultMaterial="Acrylic"
    HostHorizontalAlignment="Center"
    HostSpacing="10"
    HostPadding="0,0,0,8" />
```

> The host auto-connects to the `INotificationQueue` registered by `AddNotificationServices()` — no manual XAML wiring needed.

### 3. Show notifications

```csharp
// One-liners
_notifications.Success("Operation completed!");
_notifications.Warning("Check your connection.");
_notifications.Error("Something went wrong!", durationMs: 5000);

// Progress
var handle = _notifications.ShowProgress("Uploading...", progress: 0);
handle.Report(50, "Uploading... 50%");
handle.Complete("Upload finished!");

// Persistent
var persistent = _notifications.ShowPersistent(
    "No connection. Retrying...",
    level: NotificationLevel.Warning,
    isClosable: false);
// later...
persistent.Dismiss();

// Full control
_notifications.Show(new NotificationRequest
{
    Message = "Sending file...",
    Level = NotificationLevel.Info,
    IsInProgress = true,
    ActionText = "Cancel",
    ActionCommand = MyCancelCommand,
    DurationMs = 3000
});
```

## Host Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `HostPosition` | `Top` / `Bottom` | `Bottom` | Where notifications stack |
| `BarStyle` | `Fluent` / `AccentStrip` | `Fluent` (WinUI) / `AccentStrip` (WPF) | Visual style |
| `DefaultDurationMs` | `int` | `3000` | Auto-dismiss duration (ms) |
| `DefaultTransition` | `SlideUp` / `Fade` / `Scale` / `SlideAndFade` | `SlideAndFade` | Animation |
| `DefaultMaterial` | `Solid` / `Acrylic` / `Mica` | `Acrylic` | Background material |
| `HostHorizontalAlignment` | `Left` / `Center` / `Right` / `Stretch` | `Center` | Horizontal alignment |
| `HostSpacing` | `double` | `8` | Gap between stacked notifications |
| `HostPadding` | `Thickness` | `0` | Additional padding around the stack |
| `NotificationWidth` | `double` | `0` (auto) | Fixed notification width |
| `NotificationMaxWidth` | `double` | `0` (none) | Maximum notification width |

## Requirements

- .NET 8, .NET 9, or .NET 10
- Windows App SDK 1.8+ (WinUI)

> **Note:** The sample apps and unit tests target .NET 10 and require the .NET 10 SDK to build and run.

## License

[MIT](LICENSE) — Jose Valencia
