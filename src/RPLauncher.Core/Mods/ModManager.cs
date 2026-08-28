using RPLauncher.Core.Logging;
using RPLauncher.Core.Models;
using RPLauncher.Core.Network;
using RPLauncher.Core.Security;

namespace RPLauncher.Core.Mods;

public enum ModActionType { Download, Delete, Keep, Update }

public record ModAction(ModActionType Type, ModEntry? Mod, string? ExistingFileName);

public record ModSyncPlan(List<ModAction> Actions, long TotalDownloadBytes);

public class ModSyncProgress
{
    public string CurrentFile { get; set; } = "";
    public int FilesDone { get; set; }
    public int FilesTotal { get; set; }
    public long BytesDone { get; set; }
    public long BytesTotal { get; set; }
}

public class ModManager
{
    private readonly Downloader _downloader;

    public ModManager(Downloader downloader)
    {
        _downloader = downloader;
    }

    public ModSyncPlan BuildPlan(ModpackManifest manifest, string rpModsDirectory)
    {
        Directory.CreateDirectory(rpModsDirectory);

        var actions = new List<ModAction>();
        long totalBytes = 0;

        var existingFiles = Directory.GetFiles(rpModsDirectory, "*.zip")
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .Select(f => f!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var manifestFileNames = manifest.Mods.Select(m => m.File).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var mod in manifest.Mods)
        {
            var localPath = Path.Combine(rpModsDirectory, mod.File);
            if (!File.Exists(localPath))
            {
                actions.Add(new ModAction(ModActionType.Download, mod, null));
                totalBytes += mod.Size;
                continue;
            }

            var localInfo = new FileInfo(localPath);
            var sizeMatches = mod.Size <= 0 || localInfo.Length == mod.Size;

            if (!sizeMatches)
            {
                actions.Add(new ModAction(ModActionType.Update, mod, mod.File));
                totalBytes += mod.Size;
                continue;
            }

            actions.Add(new ModAction(ModActionType.Keep, mod, mod.File));
        }

        foreach (var existing in existingFiles)
        {
            if (!manifestFileNames.Contains(existing))
            {
                actions.Add(new ModAction(ModActionType.Delete, null, existing));
            }
        }

        return new ModSyncPlan(actions, totalBytes);
    }

    public async Task ApplyPlanAsync(
        ModSyncPlan plan,
        string rpModsDirectory,
        string baseDownloadUrl,
        IProgress<ModSyncProgress>? progress = null,
        CancellationToken ct = default)
    {
        var toProcess = plan.Actions.Where(a => a.Type is ModActionType.Download or ModActionType.Update).ToList();
        var progressState = new ModSyncProgress { FilesTotal = toProcess.Count, BytesTotal = plan.TotalDownloadBytes };

        foreach (var action in toProcess)
        {
            ct.ThrowIfCancellationRequested();
            var mod = action.Mod!;
            var destination = Path.Combine(rpModsDirectory, mod.File);
            var url = mod.Url ?? CombineUrl(baseDownloadUrl, mod.File);

            progressState.CurrentFile = mod.File;
            progress?.Report(progressState);

            var fileProgress = new Progress<DownloadProgress>(p =>
            {
                progressState.BytesDone = p.BytesReceived;
                progress?.Report(progressState);
            });

            await _downloader.DownloadFileAsync(url, destination, fileProgress, ct);

            var valid = await HashVerifier.VerifyAsync(destination, mod.Sha256, ct);
            if (!valid)
            {
                File.Delete(destination);
                throw new InvalidDataException($"Le fichier téléchargé '{mod.File}' ne correspond pas au SHA-256 attendu. Fichier supprimé par sécurité.");
            }

            Logger.Info($"Mod validé (SHA-256 OK) : {mod.File}");
            progressState.FilesDone++;
        }

        foreach (var action in plan.Actions.Where(a => a.Type == ModActionType.Delete))
        {
            var path = Path.Combine(rpModsDirectory, action.ExistingFileName!);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Logger.Info($"Mod obsolète supprimé : {action.ExistingFileName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Impossible de supprimer '{action.ExistingFileName}' : {ex.Message}");
            }
        }
    }

    public async Task<List<string>> VerifyAllAsync(ModpackManifest manifest, string rpModsDirectory, CancellationToken ct = default)
    {
        var corrupted = new List<string>();
        foreach (var mod in manifest.Mods)
        {
            var path = Path.Combine(rpModsDirectory, mod.File);
            var valid = await HashVerifier.VerifyAsync(path, mod.Sha256, ct);
            if (!valid)
            {
                corrupted.Add(mod.File);
            }
        }
        return corrupted;
    }

    private static string CombineUrl(string baseUrl, string fileName)
    {
        return baseUrl.TrimEnd('/') + "/" + Uri.EscapeDataString(fileName);
    }
}
