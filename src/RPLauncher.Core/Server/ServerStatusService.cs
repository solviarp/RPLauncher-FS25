using System.Text.Json.Serialization;
using RPLauncher.Core.Network;

namespace RPLauncher.Core.Server;

public class ServerStatusData
{
    [JsonPropertyName("online")]
    public bool Online { get; set; }

    [JsonPropertyName("players")]
    public int Players { get; set; }

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ServerStatusService
{
    private readonly Downloader _downloader;

    public ServerStatusService(Downloader downloader)
    {
        _downloader = downloader;
    }

    public async Task<ServerStatusData?> FetchAsync(string? statusUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(statusUrl)) return null;

        try
        {
            var json = await _downloader.DownloadTextAsync(statusUrl, ct);
            return System.Text.Json.JsonSerializer.Deserialize<ServerStatusData>(json);
        }
        catch
        {
            return null;
        }
    }
}
