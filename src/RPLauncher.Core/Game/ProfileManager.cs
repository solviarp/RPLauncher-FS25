using System.Xml.Linq;
using RPLauncher.Core.Logging;

namespace RPLauncher.Core.Game;

public static class ProfileManager
{
    public const string DefaultRpProfileFolderName = "FarmingSimulator2025_RP";

    public static string GetDefaultDocumentsGamesFolder()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docs, "My Games");
    }

    public static string GetDefaultProfilePath()
    {
        return Path.Combine(GetDefaultDocumentsGamesFolder(), "FarmingSimulator2025");
    }

    public static (string ProfilePath, string ModsPath) CreateOrRepairRpProfile(string? customBasePath = null)
    {
        var basePath = customBasePath ?? GetDefaultDocumentsGamesFolder();
        var profilePath = Path.Combine(basePath, DefaultRpProfileFolderName);
        var modsPath = Path.Combine(profilePath, "mods");

        Directory.CreateDirectory(profilePath);
        Directory.CreateDirectory(modsPath);

        WriteGameSettings(profilePath, modsPath);

        Logger.Info($"Profil RP prêt : {profilePath}");
        return (profilePath, modsPath);
    }

    private static void WriteGameSettings(string profilePath, string modsPath)
    {
        var settingsPath = Path.Combine(profilePath, "gameSettings.xml");

        XDocument doc;
        if (File.Exists(settingsPath))
        {
            try
            {
                doc = XDocument.Load(settingsPath);
            }
            catch
            {
                doc = CreateMinimalGameSettings();
            }
        }
        else
        {
            doc = CreateMinimalGameSettings();
        }

        var root = doc.Root!;
        var overrideElement = root.Element("modsDirectoryOverride");
        if (overrideElement is null)
        {
            overrideElement = new XElement("modsDirectoryOverride");
            root.Add(overrideElement);
        }

        overrideElement.SetAttributeValue("active", "true");
        overrideElement.SetAttributeValue("directory", modsPath);

        doc.Save(settingsPath);
    }

    private static XDocument CreateMinimalGameSettings()
    {
        return new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("gameSettings",
                new XElement("modsDirectoryOverride",
                    new XAttribute("active", "true"),
                    new XAttribute("directory", ""))
            )
        );
    }

    public static bool IsSeparateFromPersonalProfile(string rpProfilePath)
    {
        var personal = GetDefaultProfilePath();
        return !string.Equals(
            Path.GetFullPath(rpProfilePath).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(personal).TrimEnd(Path.DirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
    }
}
