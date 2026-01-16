using FureFenyx.WinUI.Notifications.SampleApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FureFenyx.WinUI.Notifications.SampleApp;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<MainViewModel>()!;
        Root.DataContext = ViewModel;
    }

    private void Success_Click(object sender, RoutedEventArgs e)
    => ViewModel.ShowSuccess();

    private void Warning_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowWarning();

    private void Error_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowError();

    private void Progress_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowProgress();

}
