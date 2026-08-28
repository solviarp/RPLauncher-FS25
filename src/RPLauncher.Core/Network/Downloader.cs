using RPLauncher.Core.Logging;

namespace RPLauncher.Core.Network;

public record DownloadProgress(string FileName, long BytesReceived, long TotalBytes);

public class Downloader
{
    private readonly HttpClient _http;

    public Downloader(HttpClient? httpClient = null)
    {
        _http = httpClient ?? new HttpClient();
        _http.Timeout = TimeSpan.FromMinutes(10);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("RPLauncher/1.0");
    }

    public async Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken ct = default)
    {
        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Téléchargement refusé : l'URL n'est pas en HTTPS ({url})");
        }

        var tempPath = destinationPath + ".part";
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var fileName = Path.GetFileName(destinationPath);

        await using (var httpStream = await response.Content.ReadAsStreamAsync(ct))
        await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await httpStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                totalRead += bytesRead;
                progress?.Report(new DownloadProgress(fileName, totalRead, totalBytes));
            }
        }

        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
        File.Move(tempPath, destinationPath);
        Logger.Info($"Téléchargement terminé : {fileName}");
    }

    public async Task<string> DownloadTextAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Requête refusée : l'URL n'est pas en HTTPS ({url})");
        }

        return await _http.GetStringAsync(url, ct);
    }
}
