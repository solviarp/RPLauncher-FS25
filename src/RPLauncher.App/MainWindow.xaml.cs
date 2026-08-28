using System.Windows;
using RPLauncher.App.Views;
using RPLauncher.Core.Game;

namespace RPLauncher.App;

public partial class MainWindow : Window
{
    private readonly AppServices _services = new();
    private HomeView? _homeView;
    private ModpackView? _modpackView;
    private SettingsView? _settingsView;
    private LogsView? _logsView;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_services.Config.FirstRunCompleted)
        {
            var wizard = new SetupWizardWindow(_services) { Owner = this };
            var result = wizard.ShowDialog();
            if (result != true || !wizard.Completed)
            {
                Application.Current.Shutdown();
                return;
            }
        }
        else
        {
            _services.Installation = ResolveInstallation();
        }

        ShowHome();
    }

    private GameInstallation? ResolveInstallation()
    {
        if (string.IsNullOrWhiteSpace(_services.Config.GameInstallPath))
        {
            return GameDetector.TryAutoDetect();
        }

        return GameDetector.ValidateManualPath(_services.Config.GameInstallPath)
               ?? GameDetector.TryAutoDetect();
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (MainContent is null) return;

        if (sender == NavHome) ShowHome();
        else if (sender == NavModpack) ShowModpack();
        else if (sender == NavSettings) ShowSettings();
        else if (sender == NavLogs) ShowLogs();
    }

    private void ShowHome()
    {
        _homeView ??= new HomeView(_services);
        _homeView.RefreshRequested();
        MainContent.Content = _homeView;
    }

    private void ShowModpack()
    {
        _modpackView ??= new ModpackView(_services);
        _modpackView.RefreshRequested();
        MainContent.Content = _modpackView;
    }

    private void ShowSettings()
    {
        _settingsView ??= new SettingsView(_services, RestartWizard);
        MainContent.Content = _settingsView;
    }

    private void ShowLogs()
    {
        _logsView ??= new LogsView();
        MainContent.Content = _logsView;
    }

    private void RestartWizard()
    {
        var wizard = new SetupWizardWindow(_services) { Owner = this };
        if (wizard.ShowDialog() == true && wizard.Completed)
        {
            _services.Installation = ResolveInstallation();
            ShowHome();
            NavHome.IsChecked = true;
        }
    }
}
