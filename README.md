# RP Launcher — Farming Simulator 25

Launcher Windows pour rejoindre un serveur Farming Simulator 25 en Role Play avec un modpack géré automatiquement, **sans jamais toucher aux mods personnels du joueur**.

Le joueur installe le launcher une seule fois. Ensuite le launcher :

- détecte l'installation de FS25 (Steam ou GIANTS) ;
- crée un **profil FS25 séparé**, dédié au serveur RP, avec son propre dossier de mods ;
- télécharge/synchronise le modpack depuis un manifeste JSON hébergé sur GitHub ;
- vérifie chaque fichier par SHA-256 avant de l'utiliser ;
- lance FS25 avec ce profil isolé ;
- laisse le joueur retrouver son installation normale (mods perso, autres serveurs) le reste du temps, sans rien avoir modifié.

## Comment fonctionne l'isolation des mods

FS25 stocke chaque configuration de jeu dans un **profil**, normalement `Documents\My Games\FarmingSimulator2025\`. Le paramètre de lancement officiel `-profile "chemin"` permet d'indiquer un autre dossier de profil — c'est ce que Farming Simulator utilise lui-même pour ses propres serveurs dédiés.

Le launcher crée donc un second profil, `FarmingSimulator2025_RP`, avec son propre `gameSettings.xml` contenant une balise `modsDirectoryOverride` pointant vers un dossier de mods dédié. Le profil par défaut du joueur (et ses mods personnels) n'est jamais modifié, déplacé ou supprimé.

**Important :** ce mécanisme est basé sur le comportement réel du moteur du jeu (confirmé par plusieurs retours de la communauté et par l'usage qu'en font les serveurs dédiés officiels), mais il ne fait pas l'objet d'une documentation GIANTS Developer Network exhaustive. Avant de distribuer le launcher à vos joueurs, **validez-le vous-même** avec le prototype décrit dans [`docs/DEVELOPER.md`](docs/DEVELOPER.md#valider-lisolation-avant-mise-en-production).

## Pour les joueurs

Voir [`docs/INSTALLATION.md`](docs/INSTALLATION.md).

En résumé : téléchargez `RPLauncher-Setup-x.x.x.exe` depuis l'onglet **Releases** de ce dépôt, installez, lancez, suivez l'assistant, cliquez sur JOUER.

## Pour l'administrateur du serveur

1. Placez vos fichiers de mods `.zip` dans un dossier.
2. Générez le manifeste avec le script fourni :
   ```powershell
   .\tools\generate-manifest.ps1 -ModsFolder "C:\chemin\vers\mods" -Version "1.0.0"
   ```
3. Publiez les `.zip` dans une **Release GitHub** de ce dépôt (ou d'un autre dépôt), et notez l'URL de base des fichiers.
4. Committez `modpack.json` (par exemple à la racine ou dans `/manifest/modpack.json`) et donnez aux joueurs l'URL **raw** de ce fichier (`https://raw.githubusercontent.com/...`) — c'est celle qu'ils entreront dans l'assistant de configuration du launcher.
5. À chaque mise à jour du modpack : changez la version, régénérez le manifeste, republiez les fichiers modifiés en Release, et poussez le nouveau `modpack.json`.

Voir [`examples/modpack.json`](examples/modpack.json) pour le format complet.

## Compiler soi-même / publier une nouvelle version du launcher

Vous n'avez rien à compiler à la main : un tag `vX.Y.Z` poussé sur ce dépôt déclenche automatiquement (`.github/workflows/release.yml`) :

1. la compilation en `.exe` autonome (aucune dépendance .NET à installer côté joueur) ;
2. la génération de l'installeur Windows avec Inno Setup ;
3. la publication d'une Release GitHub avec `RPLauncher-Setup-X.Y.Z.exe` en pièce jointe.

```bash
git tag v1.0.0
git push origin v1.0.0
```

Détails techniques complets : [`docs/DEVELOPER.md`](docs/DEVELOPER.md).

## Structure du projet

```
RPLauncher.sln
src/
  RPLauncher.Core/     Logique métier (rien de graphique) : manifeste, mods, hash, détection du jeu, lancement, profils
  RPLauncher.App/      Interface WPF sombre (Accueil, Modpack, Paramètres, Logs)
installer/
  setup.iss            Script Inno Setup
.github/workflows/
  release.yml          Build + installeur + release automatiques
examples/
  modpack.json          Exemple de manifeste
tools/
  generate-manifest.ps1 Génère modpack.json avec les bons SHA-256 à partir d'un dossier de mods
docs/
  INSTALLATION.md       Guide pour les joueurs
  DEVELOPER.md          Guide pour les développeurs / administrateurs du serveur
```

## Sécurité

- Tous les téléchargements sont forcés en HTTPS.
- Chaque fichier de mod est vérifié par SHA-256 avant d'être considéré comme valide ; un fichier invalide est supprimé automatiquement plutôt qu'utilisé.
- Le manifeste est validé (structure, hash présents, pas de chemins suspects dans les noms de fichiers) avant d'être utilisé.
- Le launcher ne télécharge et n'exécute jamais de script arbitraire : seuls les `.zip` de mods déclarés dans le manifeste sont manipulés, comme données, jamais comme code.

## Limites connues (honnêtement)

- Le statut "joueurs en ligne / X" dépend d'une URL `statusUrl` que **vous devez héberger vous-même** (un simple fichier JSON `{ "online": true, "players": 12, "maxPlayers": 32 }`) : il n'existe pas d'API publique officielle FS25 pour interroger un serveur dédié à distance.
- Pour la version Steam, FS25 vérifie généralement que le client Steam tourne en arrière-plan (protection standard) ; le launcher affiche un avertissement si Steam n'est pas détecté au moment du lancement.
- Le rollback automatique après une mise à jour ratée n'est pas encore implémenté (seule la vérification/réparation manuelle l'est) — voir la section "Pistes d'amélioration" du guide développeur.
