using FureFenyx.WinUI.Notifications.SampleApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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

    private void Countdown_Click(object sender, RoutedEventArgs e)
        => ViewModel.StartCountdown();

    private void CancelCountdown_Click(object sender, RoutedEventArgs e)
        => ViewModel.CancelCountdown();

    private void SendFile_Click(object sender, RoutedEventArgs e)
        => ViewModel.SendFileComplexScenario();

    private void ShowPersistent_Click(object sender, RoutedEventArgs e)
        => ViewModel.ShowPersistentNoConnection();

    private void DismissPersistent_Click(object sender, RoutedEventArgs e)
        => ViewModel.DismissPersistent();

}
