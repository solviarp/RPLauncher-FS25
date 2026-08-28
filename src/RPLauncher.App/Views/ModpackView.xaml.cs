using System.Windows;
using System.Windows.Controls;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Mods;

namespace RPLauncher.App.Views;

public partial class ModpackView : UserControl
{
    private readonly AppServices _services;
    private bool _busy;

    public ModpackView(AppServices services)
    {
        InitializeComponent();
        _services = services;
    }

    public async void RefreshRequested()
    {
        if (_services.LastManifest is null)
        {
            try
            {
                _services.LastManifest = await _services.ManifestService.FetchAsync(_services.Config.ManifestUrl);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Impossible de charger le manifeste pour la vue Modpack : {ex.Message}");
            }
        }

        RenderManifest();
    }

    private void RenderManifest()
    {
        ChangelogPanel.Children.Clear();
        var manifest = _services.LastManifest;

        if (manifest is null)
        {
            VersionText.Text = "Version : indisponible hors ligne";
            ModsCountText.Text = "";
            return;
        }

        VersionText.Text = $"Version installée : {_services.Config.InstalledModpackVersion} — Dernière version : {manifest.Version}";
        ModsCountText.Text = $"{manifest.Mods.Count} mods dans le modpack";

        foreach (var entry in manifest.Changelog)
        {
            var card = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("BgCard"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = $"v{entry.Version}", FontWeight = FontWeights.SemiBold, FontSize = 14 });

            AddChangeList(stack, "Nouveautés", entry.Added);
            AddChangeList(stack, "Corrections", entry.Fixed);
            AddChangeList(stack, "Mods supprimés", entry.Removed);

            card.Child = stack;
            ChangelogPanel.Children.Add(card);
        }
    }

    private void AddChangeList(StackPanel parent, string title, List<string> items)
    {
        if (items.Count == 0) return;

        parent.Children.Add(new TextBlock
        {
            Text = title,
            Style = (Style)FindResource("SubtleText"),
            Margin = new Thickness(0, 8, 0, 2)
        });

        foreach (var item in items)
        {
            parent.Children.Add(new TextBlock { Text = $"• {item}", Margin = new Thickness(8, 0, 0, 0) });
        }
    }

    private async void Repair_Click(object sender, RoutedEventArgs e)
    {
        if (_busy || _services.Config.RpModsPath is null) return;
        _busy = true;
        RepairProgressBar.Visibility = Visibility.Visible;
        RepairStatusText.Text = "Relecture du manifeste...";

        try
        {
            var manifest = await _services.ManifestService.FetchAsync(_services.Config.ManifestUrl);
            _services.LastManifest = manifest;

            RepairStatusText.Text = "Vérification des fichiers...";
            var corrupted = await _services.ModManager.VerifyAllAsync(manifest, _services.Config.RpModsPath);

            if (corrupted.Count == 0)
            {
                RepairStatusText.Text = "Tous les fichiers sont valides.";
                RenderManifest();
                return;
            }

            RepairStatusText.Text = $"{corrupted.Count} fichier(s) manquant(s) ou corrompu(s), retéléchargement...";

            foreach (var fileName in corrupted)
            {
                var path = Path.Combine(_services.Config.RpModsPath, fileName);
                if (File.Exists(path)) File.Delete(path);
            }

            var plan = _services.ModManager.BuildPlan(manifest, _services.Config.RpModsPath);
            var progress = new Progress<ModSyncProgress>(p =>
            {
                RepairProgressBar.Value = p.BytesTotal > 0 ? (double)p.BytesDone / p.BytesTotal * 100.0 : 0;
                RepairStatusText.Text = $"{p.CurrentFile} ({p.FilesDone}/{p.FilesTotal})";
            });

            await _services.ModManager.ApplyPlanAsync(plan, _services.Config.RpModsPath, manifest.BaseDownloadUrl, progress);

            _services.Config.InstalledModpackVersion = manifest.Version;
            _services.SaveConfig();

            RepairStatusText.Text = "Réparation terminée.";
            RenderManifest();
        }
        catch (Exception ex)
        {
            Logger.Error("Échec de la réparation", ex);
            RepairStatusText.Text = $"Échec de la réparation : {ex.Message}";
        }
        finally
        {
            _busy = false;
            RepairProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
