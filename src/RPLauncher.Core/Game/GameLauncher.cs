using System.Diagnostics;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Models;

namespace RPLauncher.Core.Game;

public class GameLaunchResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public static class GameLauncher
{
    public static GameLaunchResult Launch(GameInstallation installation, string rpProfilePath, string? serverAddress, int serverPort)
    {
        try
        {
            if (installation.Platform == GamePlatform.Steam && !IsSteamRunning())
            {
                Logger.Warning("Steam ne semble pas lancé. Le jeu peut refuser de démarrer (vérification DRM Steam).");
            }

            var args = $"-profile \"{rpProfilePath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = installation.GameExecutablePath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(installation.GameExecutablePath),
                UseShellExecute = false
            };

            Logger.Info($"Lancement : \"{startInfo.FileName}\" {args}");
            var process = Process.Start(startInfo);

            if (process is null)
            {
                return new GameLaunchResult { Success = false, ErrorMessage = "Le processus n'a pas pu démarrer." };
            }

            return new GameLaunchResult { Success = true };
        }
        catch (Exception ex)
        {
            Logger.Error("Échec du lancement de FS25", ex);
            return new GameLaunchResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static bool IsSteamRunning()
    {
        try
        {
            return Process.GetProcessesByName("steam").Length > 0;
        }
        catch
        {
            return true;
        }
    }
}
