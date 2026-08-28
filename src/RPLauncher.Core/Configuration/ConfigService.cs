using System.Text.Json;
using RPLauncher.Core.Logging;
using RPLauncher.Core.Models;

namespace RPLauncher.Core.Configuration;

public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string AppDataDirectory { get; }
    public string ConfigFilePath { get; }
    public string LogsDirectory { get; }
    public string BackupsDirectory { get; }

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        AppDataDirectory = Path.Combine(appData, "RPLauncher");
        ConfigFilePath = Path.Combine(AppDataDirectory, "config.json");
        LogsDirectory = Path.Combine(AppDataDirectory, "logs");
        BackupsDirectory = Path.Combine(AppDataDirectory, "backups");

        Directory.CreateDirectory(AppDataDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(BackupsDirectory);
    }

    public AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                if (config is not null) return config;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Impossible de lire la configuration, valeurs par défaut utilisées : {ex.Message}");
        }

        return new AppConfig();
    }

    public void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            Logger.Error("Échec de la sauvegarde de la configuration", ex);
        }
    }
}
