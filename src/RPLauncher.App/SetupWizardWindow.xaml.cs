using System.Windows;
using Microsoft.Win32;
using RPLauncher.Core.Game;
using RPLauncher.Core.Models;

namespace RPLauncher.App;

public partial class SetupWizardWindow : Window
{
    private readonly AppServices _services;
    private GameInstallation? _detected;

    public bool Completed { get; private set; }

    public SetupWizardWindow(AppServices services)
    {
        InitializeComponent();
        _services = services;

        RpProfileBasePathTextBox.Text = ProfileManager.GetDefaultDocumentsGamesFolder();
        ManifestUrlTextBox.Text = _services.Config.ManifestUrl;

        Loaded += (_, _) => RunDetection();
    }

    private void RunDetection()
    {
        _detected = GameDetector.TryAutoDetect();

        if (_detected is not null)
        {
            DetectionResultText.Text = $"Installation détectée automatiquement ({_detected.Platform}).";
            GamePathTextBox.Text = _detected.InstallDirectory;
        }
        else
        {
            DetectionResultText.Text = "Installation non détectée automatiquement. Sélectionnez le dossier d'installation de Farming Simulator 25 (celui qui contient le dossier \"x64\").";
        }
    }

    private void BrowseGamePath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Sélectionnez le dossier d'installation de Farming Simulator 25"
        };

        if (dialog.ShowDialog() == true)
        {
            var validated = GameDetector.ValidateManualPath(dialog.FolderName);
            if (validated is null)
            {
                ErrorText.Text = "Ce dossier ne contient pas FarmingSimulator2025Game.exe (attendu dans un sous-dossier x64).";
                return;
            }

            ErrorText.Text = "";
            _detected = validated;
            GamePathTextBox.Text = validated.InstallDirectory;
            DetectionResultText.Text = "Installation sélectionnée manuellement.";
        }
    }

    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        if (_detected is null)
        {
            ErrorText.Text = "Merci d'indiquer l'emplacement de Farming Simulator 25 avant de continuer.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ManifestUrlTextBox.Text) || !ManifestUrlTextBox.Text.StartsWith("https://"))
        {
            ErrorText.Text = "L'adresse du manifeste doit être une URL en https://.";
            return;
        }

        var (profilePath, modsPath) = ProfileManager.CreateOrRepairRpProfile(RpProfileBasePathTextBox.Text);

        _services.Config.GameInstallPath = _detected.InstallDirectory;
        _services.Config.GamePlatform = _detected.Platform;
        _services.Config.RpProfilePath = profilePath;
        _services.Config.RpModsPath = modsPath;
        _services.Config.ManifestUrl = ManifestUrlTextBox.Text.Trim();
        _services.Config.FirstRunCompleted = true;
        _services.Installation = _detected;
        _services.SaveConfig();

        Completed = true;
        DialogResult = true;
        Close();
    }
}
