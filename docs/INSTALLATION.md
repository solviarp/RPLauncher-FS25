# Installation — Joueurs

## 1. Télécharger

Allez dans l'onglet **Releases** de ce dépôt GitHub et téléchargez le fichier `RPLauncher-Setup-X.Y.Z.exe` (prenez toujours la version la plus récente en haut de la liste).

## 2. Installer

Double-cliquez sur le fichier téléchargé et suivez l'installateur. Aucune option particulière à cocher.

> Windows peut afficher un avertissement SmartScreen ("Windows a protégé votre ordinateur") car l'installeur n'est pas encore signé par un certificat payant. Cliquez sur **Informations complémentaires** puis **Exécuter quand même**.

## 3. Premier lancement

Au premier démarrage, un assistant s'ouvre :

1. **Installation de Farming Simulator 25** — normalement détectée automatiquement (Steam ou GIANTS). Si ce n'est pas le cas, cliquez sur **Parcourir** et sélectionnez le dossier d'installation du jeu (celui qui contient un sous-dossier `x64`).
2. **Dossier du profil RP** — laissez la valeur par défaut, sauf raison particulière.
3. **URL du manifeste** — collez l'URL donnée par l'administrateur du serveur (commence par `https://raw.githubusercontent.com/...`).

Cliquez sur **Terminer**.

## 4. Jouer

Sur l'écran d'accueil :

- Si une mise à jour du modpack est disponible, le bouton affiche **METTRE À JOUR** : le launcher télécharge et vérifie les fichiers nécessaires, puis vous pourrez cliquer à nouveau pour jouer.
- Sinon, cliquez directement sur **JOUER** : le jeu se lance avec le profil RP, vos mods personnels restent intacts et invisibles depuis ce profil.

## Vos mods personnels ne sont jamais touchés

Le launcher crée un profil FS25 séparé (`FarmingSimulator2025_RP`) avec son propre dossier de mods. Votre profil habituel, vos sauvegardes et vos mods personnels restent exactement là où ils étaient.

## En cas de problème

- Onglet **Réparer l'installation** (page Modpack) : revérifie tous les fichiers du modpack et retélécharge ceux qui posent problème, sans jamais toucher à vos mods perso.
- Onglet **Logs** : affiche l'historique détaillé, utile si vous devez signaler un bug à l'administrateur du serveur.
- Si Farming Simulator refuse de se lancer alors que vous êtes sur la version Steam, vérifiez que **Steam est bien ouvert** avant de cliquer sur JOUER.
