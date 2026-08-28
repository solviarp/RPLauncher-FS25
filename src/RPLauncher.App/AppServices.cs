using RPLauncher.Core.Configuration;
using RPLauncher.Core.Game;
using RPLauncher.Core.Manifest;
using RPLauncher.Core.Mods;
using RPLauncher.Core.Models;
using RPLauncher.Core.Network;
using RPLauncher.Core.Server;

namespace RPLauncher.App;

public class AppServices
{
    public ConfigService ConfigService { get; } = new();
    public AppConfig Config { get; set; }
    public Downloader Downloader { get; } = new();
    public ManifestService ManifestService { get; }
    public ModManager ModManager { get; }
    public ServerStatusService ServerStatusService { get; }

    public GameInstallation? Installation { get; set; }
    public ModpackManifest? LastManifest { get; set; }

    public AppServices()
    {
        Config = ConfigService.Load();
        ManifestService = new ManifestService(Downloader);
        ModManager = new ModManager(Downloader);
        ServerStatusService = new ServerStatusService(Downloader);
    }

    public void SaveConfig() => ConfigService.Save(Config);
}
