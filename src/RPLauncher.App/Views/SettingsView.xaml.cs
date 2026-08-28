using System.Windows;
using System.Windows.Controls;

namespace RPLauncher.App.Views;

public partial class SettingsView : UserControl
{
    private readonly AppServices _services;
    private readonly Action _onRelaunchWizard;

    public SettingsView(AppServices services, Action onRelaunchWizard)
    {
        InitializeComponent();
        _services = services;
        _onRelaunchWizard = onRelaunchWizard;

        GamePathText.Text = string.IsNullOrWhiteSpace(_services.Config.GameInstallPath)
            ? "Non configuré"
            : _services.Config.GameInstallPath;

        RpProfilePathText.Text = string.IsNullOrWhiteSpace(_services.Config.RpProfilePath)
            ? "Non configuré"
            : _services.Config.RpProfilePath;

        ManifestUrlTextBox.Text = _services.Config.ManifestUrl;
    }

    private void RelaunchWizard_Click(object sender, RoutedEventArgs e) => _onRelaunchWizard();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _services.Config.ManifestUrl = ManifestUrlTextBox.Text.Trim();
        _services.SaveConfig();
        SavedText.Visibility = Visibility.Visible;
    }
}
