using System.Text.Json.Serialization;

namespace RPLauncher.Core.Models;

public class ModpackManifest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.0.0";

    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; set; } = "FS25";

    [JsonPropertyName("serverAddress")]
    public string ServerAddress { get; set; } = "";

    [JsonPropertyName("serverPort")]
    public int ServerPort { get; set; }

    [JsonPropertyName("statusUrl")]
    public string? StatusUrl { get; set; }

    [JsonPropertyName("baseDownloadUrl")]
    public string BaseDownloadUrl { get; set; } = "";

    [JsonPropertyName("changelog")]
    public List<ChangelogEntry> Changelog { get; set; } = new();

    [JsonPropertyName("mods")]
    public List<ModEntry> Mods { get; set; } = new();
}

public class ChangelogEntry
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("added")]
    public List<string> Added { get; set; } = new();

    [JsonPropertyName("removed")]
    public List<string> Removed { get; set; } = new();

    [JsonPropertyName("fixed")]
    public List<string> Fixed { get; set; } = new();
}

public class ModEntry
{
    [JsonPropertyName("file")]
    public string File { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = "";

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
