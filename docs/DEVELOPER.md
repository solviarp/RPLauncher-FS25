# Guide développeur

## Pile technique

- .NET 8, C#
- `RPLauncher.Core` : bibliothèque de classes, aucune dépendance UI (facilement testable / réutilisable, par exemple pour un futur outil en ligne de commande)
- `RPLauncher.App` : application WPF (`net8.0-windows`), thème sombre défini dans `Themes/Dark.xaml`
- Compilation en exécutable unique auto-suffisant (`PublishSingleFile`), le joueur n'installe rien d'autre

## Compiler en local

Prérequis : [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), Windows (WPF ne compile que sur Windows).

```powershell
dotnet restore
dotnet build -c Release
```

Pour reproduire exactement ce que fait la release automatique (exécutable autonome) :

```powershell
dotnet publish src/RPLauncher.App/RPLauncher.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Puis, avec [Inno Setup](https://jrsoftware.org/isinfo.php) installé :

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\setup.iss
```

## Publier une nouvelle version

```bash
git tag v1.1.0
git push origin v1.1.0
```

Le workflow `.github/workflows/release.yml` s'occupe de tout (build, installeur, release GitHub avec le `.exe` en pièce jointe).

## Architecture de `RPLauncher.Core`

| Dossier | Rôle |
|---|---|
| `Models` | `ModpackManifest`, `ModEntry`, `AppConfig` |
| `Manifest` | Récupération et validation du `modpack.json` distant |
| `Mods` | Calcul du diff (à télécharger / mettre à jour / supprimer) et application |
| `Network` | Téléchargement HTTP (HTTPS uniquement, avec progression) |
| `Security` | Vérification SHA-256 |
| `Game` | Détection de l'installation FS25, gestion du profil RP isolé, lancement du jeu |
| `Server` | Lecture optionnelle d'un statut serveur (JSON hébergé par l'administrateur) |
| `Configuration` | Lecture/écriture de la config locale (`%AppData%\RPLauncher\config.json`) |
| `Logging` | Logs fichier + événement pour l'UI |

## Valider l'isolation avant mise en production

Le comportement de `-profile` et `modsDirectoryOverride` est confirmé par l'usage réel (y compris par les serveurs dédiés FS25 eux-mêmes), mais n'est pas exhaustivement documenté par GIANTS. **Avant de distribuer le launcher à vos joueurs**, faites ce test manuel une fois :

1. Lancez le launcher, terminez l'assistant.
2. Allez dans `Documents\My Games\FarmingSimulator2025_RP\` et vérifiez qu'un `gameSettings.xml` a été créé avec une balise `modsDirectoryOverride active="true"` pointant vers le sous-dossier `mods` de ce même profil.
3. Placez un mod de test dans ce dossier `mods`, cliquez sur JOUER.
4. Dans FS25, ouvrez le menu des mods : seul le mod de test doit apparaître, pas vos mods personnels habituels.
5. Quittez FS25, relancez-le **normalement** (pas via RP Launcher) : vérifiez que vos mods personnels sont toujours là et que le mod de test n'apparaît pas.

Si un de ces points échoue, ouvrez une issue sur ce dépôt avec la version de FS25 et la plateforme (Steam/GIANTS) concernée — le mécanisme peut varier selon les mises à jour du jeu.

## Pistes d'amélioration (non implémentées volontairement, cf. cahier des charges initial)

- Authentification Discord, API serveur, liste de joueurs en direct, whitelist, comptes, statistiques, événements, votes, notifications, multi-serveurs.
- Rollback automatique vers l'ancienne version du modpack en cas d'échec de mise à jour (actuellement : réparation manuelle uniquement).
- Signature cryptographique du manifeste (au-delà du SHA-256 par fichier).
- Rejoindre automatiquement le serveur dès le lancement du jeu (aucune méthode officielle de "auto-join" confirmée à ce jour — à vérifier par un nouveau prototype si souhaité).

## Où adapter le code à votre serveur

- `AppConfig.ManifestUrl` (valeur par défaut dans `Models/AppConfig.cs`) : mettez l'URL de votre `modpack.json`.
- `installer/setup.iss` : changez `AppPublisher`, l'icône si besoin, et le `AppId` (GUID) si vous forkez ce launcher pour un usage totalement différent.
- `examples/modpack.json` : à copier/adapter, `baseDownloadUrl` doit pointer vers votre Release GitHub contenant les `.zip` des mods.
