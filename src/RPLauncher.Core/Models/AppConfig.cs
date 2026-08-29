using System.Text.Json.Serialization;

namespace RPLauncher.Core.Models;

public enum GamePlatform
{
    Unknown = 0,
    Steam = 1,
    Giants = 2
}

public class AppConfig
{
    [JsonPropertyName("manifestUrl")]
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/solviarp/RPLauncher-FS25/main/modpack.json";

    [JsonPropertyName("gameInstallPath")]
    public string? GameInstallPath { get; set; }

    [JsonPropertyName("gamePlatform")]
    public GamePlatform GamePlatform { get; set; } = GamePlatform.Unknown;

    [JsonPropertyName("defaultProfilePath")]
    public string? DefaultProfilePath { get; set; }

    [JsonPropertyName("rpProfilePath")]
    public string? RpProfilePath { get; set; }

    [JsonPropertyName("rpModsPath")]
    public string? RpModsPath { get; set; }

    [JsonPropertyName("installedModpackVersion")]
    public string InstalledModpackVersion { get; set; } = "0.0.0";

    [JsonPropertyName("keepPreviousVersionDays")]
    public int KeepPreviousVersionDays { get; set; } = 5;

    [JsonPropertyName("firstRunCompleted")]
    public bool FirstRunCompleted { get; set; } = false;
}
