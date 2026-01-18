using FireFenyx.WinUI.Notifications.Extensions;
using FureFenyx.WinUI.Notifications.SampleApp.Services;
using FureFenyx.WinUI.Notifications.SampleApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FureFenyx.WinUI.Notifications.SampleApp;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    public static IServiceProvider Services { get; private set; } = default!;

    public static Window? MainWindow { get; private set; }


    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        ConfigureServices();

    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register your notification library
        services.AddNotificationServices();

        // Register ViewModels
        services.AddSingleton<MainViewModel>();

        // UI services
        services.AddSingleton<IDialogService>(_ => new ContentDialogService(() => MainWindow));

        Services = services.BuildServiceProvider();
    }


    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();
    }
}
