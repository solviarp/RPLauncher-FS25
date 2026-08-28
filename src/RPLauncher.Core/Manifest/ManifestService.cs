using System.Text.Json;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Models;
using RPLauncher.Core.Network;

namespace RPLauncher.Core.Manifest;

public class ManifestService
{
    private readonly Downloader _downloader;

    public ManifestService(Downloader downloader)
    {
        _downloader = downloader;
    }

    public async Task<ModpackManifest> FetchAsync(string manifestUrl, CancellationToken ct = default)
    {
        Logger.Info($"Récupération du manifeste : {manifestUrl}");
        var json = await _downloader.DownloadTextAsync(manifestUrl, ct);

        var manifest = JsonSerializer.Deserialize<ModpackManifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (manifest is null)
        {
            throw new InvalidDataException("Le manifeste distant est vide ou invalide.");
        }

        Validate(manifest);
        return manifest;
    }

    private static void Validate(ModpackManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            throw new InvalidDataException("Le manifeste ne contient pas de version.");
        }

        foreach (var mod in manifest.Mods)
        {
            if (string.IsNullOrWhiteSpace(mod.File))
            {
                throw new InvalidDataException("Un mod du manifeste n'a pas de nom de fichier.");
            }

            if (string.IsNullOrWhiteSpace(mod.Sha256) || mod.Sha256.Length != 64)
            {
                throw new InvalidDataException($"Le mod '{mod.File}' n'a pas de SHA-256 valide (64 caractères hexadécimaux attendus).");
            }

            if (!mod.File.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Le mod '{mod.File}' n'est pas un fichier .zip.");
            }

            if (mod.File.Contains("..") || mod.File.Contains('/') || mod.File.Contains('\\'))
            {
                throw new InvalidDataException($"Nom de fichier de mod invalide (chemin suspect) : '{mod.File}'.");
            }
        }
    }

    public static int CompareVersions(string a, string b)
    {
        var pa = ParseVersion(a);
        var pb = ParseVersion(b);
        for (int i = 0; i < 3; i++)
        {
            if (pa[i] != pb[i]) return pa[i].CompareTo(pb[i]);
        }
        return 0;
    }

    private static int[] ParseVersion(string v)
    {
        var parts = (v ?? "0.0.0").Split('.', StringSplitOptions.RemoveEmptyEntries);
        var result = new int[3];
        for (int i = 0; i < 3; i++)
        {
            result[i] = i < parts.Length && int.TryParse(parts[i], out var n) ? n : 0;
        }
        return result;
    }
}
