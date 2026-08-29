using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RPLauncher.Core.Game;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Manifest;
using RPLauncher.Core.Mods;

namespace RPLauncher.App.Views;

public partial class HomeView : UserControl
{
    private readonly AppServices _services;
    private bool _updateAvailable;
    private bool _busy;

    public HomeView(AppServices services)
    {
        InitializeComponent();
        _services = services;
    }

    public async void RefreshRequested()
    {
        if (_busy) return;
        await LoadStatusAsync();
    }

    private async Task LoadStatusAsync()
    {
        ErrorText.Visibility = Visibility.Collapsed;
        ActionButton.IsEnabled = false;

        if (_services.Installation is null)
        {
            StatusText.Text = "Farming Simulator 25 introuvable.";
            StatusDot.Fill = (Brush)FindResource("AccentRed");
            ErrorText.Text = "Corrigez le chemin du jeu dans l'onglet Paramètres.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var manifest = await _services.ManifestService.FetchAsync(_services.Config.ManifestUrl);
            _services.LastManifest = manifest;

            var statusData = await _services.ServerStatusService.FetchAsync(manifest.StatusUrl);
            if (statusData is not null)
            {
                StatusDot.Fill = (Brush)FindResource(statusData.Online ? "AccentGreen" : "AccentRed");
                StatusText.Text = statusData.Online ? "Serveur en ligne" : "Serveur hors ligne";
                PlayersText.Text = $"{statusData.Players} / {statusData.MaxPlayers} joueurs";
            }
            else
            {
                StatusDot.Fill = (Brush)FindResource("AccentAmber");
                StatusText.Text = "Statut du serveur indisponible";
                PlayersText.Text = "";
            }

            ModpackVersionText.Text = $"Modpack v{manifest.Version}";

            var cmp = ManifestService.CompareVersions(manifest.Version, _services.Config.InstalledModpackVersion);
            _updateAvailable = cmp > 0 || !HasAllModsLocally(manifest);

            ModpackStatusText.Text = _updateAvailable ? "Une mise à jour est disponible." : "Tout est à jour.";
            ModpackStatusText.Foreground = (Brush)FindResource(_updateAvailable ? "AccentAmber" : "AccentGreen");

            ActionButton.Content = _updateAvailable ? "METTRE À JOUR" : "JOUER";
            ActionButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Logger.Error("Échec de la vérification du statut", ex);
            StatusDot.Fill = (Brush)FindResource("AccentRed");
            StatusText.Text = "Impossible de contacter le serveur de mise à jour";
            ErrorText.Text = ex.Message;
            ErrorText.Visibility = Visibility.Visible;
            ActionButton.Content = "JOUER (hors ligne)";
            ActionButton.IsEnabled = _services.Config.InstalledModpackVersion != "0.0.0";
        }
    }

    private bool HasAllModsLocally(Core.Models.ModpackManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(_services.Config.RpModsPath)) return false;

        foreach (var mod in manifest.Mods)
        {
            if (!File.Exists(Path.Combine(_services.Config.RpModsPath, mod.File))) return false;
        }
        return true;
    }

    private async void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        _busy = true;
        ActionButton.IsEnabled = false;

        try
        {
            if (_updateAvailable && _services.LastManifest is not null && _services.Config.RpModsPath is not null)
            {
                await SyncModsAsync(_services.LastManifest);
            }

            LaunchGame();
        }
        finally
        {
            _busy = false;
            ActionButton.IsEnabled = true;
        }
    }

    private async Task SyncModsAsync(Core.Models.ModpackManifest manifest)
    {
        SyncProgressBar.Visibility = Visibility.Visible;
        SyncProgressText.Visibility = Visibility.Visible;
        SyncProgressBar.Value = 0;

        try
        {
            var plan = _services.ModManager.BuildPlan(manifest, _services.Config.RpModsPath!);

            var progress = new Progress<ModSyncProgress>(p =>
            {
                if (p.BytesTotal > 0)
                {
                    SyncProgressBar.Value = (double)p.BytesDone / p.BytesTotal * 100.0;
                }
                SyncProgressText.Text = $"{p.CurrentFile} ({p.FilesDone}/{p.FilesTotal})";
            });

            await _services.ModManager.ApplyPlanAsync(plan, _services.Config.RpModsPath!, manifest.BaseDownloadUrl, progress);

            _services.Config.InstalledModpackVersion = manifest.Version;
            _services.SaveConfig();

            _updateAvailable = false;
            ModpackStatusText.Text = "Tout est à jour.";
            ModpackStatusText.Foreground = (Brush)FindResource("AccentGreen");
            ActionButton.Content = "JOUER";
        }
        catch (Exception ex)
        {
            Logger.Error("Échec de la synchronisation des mods", ex);
            ErrorText.Text = $"Échec de la mise à jour du modpack : {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
            throw;
        }
        finally
        {
            SyncProgressBar.Visibility = Visibility.Collapsed;
            SyncProgressText.Visibility = Visibility.Collapsed;
        }
    }

    private void LaunchGame()
    {
        if (_services.Installation is null || string.IsNullOrWhiteSpace(_services.Config.RpProfilePath))
        {
            ErrorText.Text = "Configuration incomplète : jeu ou profil RP introuvable.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        var manifest = _services.LastManifest;
        var result = GameLauncher.Launch(
            _services.Installation,
            _services.Config.RpProfilePath,
            manifest?.ServerAddress,
            manifest?.ServerPort ?? 0);

        if (!result.Success)
        {
            ErrorText.Text = $"Impossible de lancer le jeu : {result.ErrorMessage}";
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
