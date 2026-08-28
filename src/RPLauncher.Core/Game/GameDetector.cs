using Microsoft.Win32;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Models;

namespace RPLauncher.Core.Game;

public record GameInstallation(GamePlatform Platform, string InstallDirectory, string GameExecutablePath);

public static class GameDetector
{
    private const string SteamAppId = "2300320";
    private const string GameExeName = "FarmingSimulator2025Game.exe";

    public static GameInstallation? TryAutoDetect()
    {
        var steam = TryDetectSteam();
        if (steam is not null) return steam;

        var giants = TryDetectGiants();
        if (giants is not null) return giants;

        return null;
    }

    public static GameInstallation? TryDetectSteam()
    {
        try
        {
            var steamPath = GetSteamInstallPath();
            if (steamPath is null) return null;

            var libraryFolders = GetSteamLibraryFolders(steamPath);

            foreach (var library in libraryFolders)
            {
                var candidate = Path.Combine(library, "steamapps", "common", "Farming Simulator 25");
                var exePath = Path.Combine(candidate, "x64", GameExeName);
                if (File.Exists(exePath))
                {
                    Logger.Info($"FS25 (Steam) détecté : {candidate}");
                    return new GameInstallation(GamePlatform.Steam, candidate, exePath);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Détection Steam échouée : {ex.Message}");
        }
        return null;
    }

    public static GameInstallation? TryDetectGiants()
    {
        try
        {
            var defaultPaths = new[]
            {
                @"C:\Program Files (x86)\Farming Simulator 2025",
                @"C:\Program Files\Farming Simulator 2025"
            };

            foreach (var candidate in defaultPaths)
            {
                var exePath = Path.Combine(candidate, "x64", GameExeName);
                if (File.Exists(exePath))
                {
                    Logger.Info($"FS25 (GIANTS) détecté : {candidate}");
                    return new GameInstallation(GamePlatform.Giants, candidate, exePath);
                }
            }

            var fromRegistry = TryFindFromUninstallRegistry();
            if (fromRegistry is not null) return fromRegistry;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Détection GIANTS échouée : {ex.Message}");
        }
        return null;
    }

    public static GameInstallation? ValidateManualPath(string installDirectory)
    {
        var directExe = Path.Combine(installDirectory, "x64", GameExeName);
        if (File.Exists(directExe))
        {
            return new GameInstallation(GamePlatform.Unknown, installDirectory, directExe);
        }

        var atRoot = Path.Combine(installDirectory, GameExeName);
        if (File.Exists(atRoot))
        {
            return new GameInstallation(GamePlatform.Unknown, installDirectory, atRoot);
        }

        return null;
    }

    private static string? GetSteamInstallPath()
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var key = hive.OpenSubKey(@"SOFTWARE\Valve\Steam")
                             ?? hive.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            var path = key?.GetValue("SteamPath") as string ?? key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    private static List<string> GetSteamLibraryFolders(string steamPath)
    {
        var libraries = new List<string> { steamPath };

        var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return libraries;

        try
        {
            var lines = File.ReadAllLines(vdfPath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("\"path\""))
                {
                    var parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var libPath = parts[^1].Replace("\\\\", "\\");
                        if (Directory.Exists(libPath) && !libraries.Contains(libPath))
                        {
                            libraries.Add(libPath);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Lecture libraryfolders.vdf échouée : {ex.Message}");
        }

        return libraries;
    }

    private static GameInstallation? TryFindFromUninstallRegistry()
    {
        var roots = new[]
        {
            (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
            (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
        };

        foreach (var (hive, subKeyPath) in roots)
        {
            using var uninstallKey = hive.OpenSubKey(subKeyPath);
            if (uninstallKey is null) continue;

            foreach (var subKeyName in uninstallKey.GetSubKeyNames())
            {
                using var entry = uninstallKey.OpenSubKey(subKeyName);
                var displayName = entry?.GetValue("DisplayName") as string;
                if (displayName is null || !displayName.Contains("Farming Simulator 25", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var installLocation = entry?.GetValue("InstallLocation") as string;
                if (string.IsNullOrWhiteSpace(installLocation)) continue;

                var exePath = Path.Combine(installLocation, "x64", GameExeName);
                if (File.Exists(exePath))
                {
                    return new GameInstallation(GamePlatform.Giants, installLocation, exePath);
                }
            }
        }
        return null;
    }
}
