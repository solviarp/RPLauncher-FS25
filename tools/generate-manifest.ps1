# Génère un modpack.json à partir d'un dossier contenant les fichiers .zip des mods.
# Usage : .\generate-manifest.ps1 -ModsFolder "C:\chemin\vers\mods" -Version "1.4.2" -OutFile "modpack.json"

param(
    [Parameter(Mandatory = $true)]
    [string]$ModsFolder,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$OutFile = "modpack.json",

    [string]$Name = "MonServeurRP",
    [string]$ServerAddress = "example.com",
    [int]$ServerPort = 10823,
    [string]$BaseDownloadUrl = "https://github.com/CHANGEME/CHANGEME/releases/download/modpack-v$Version"
)

if (-not (Test-Path $ModsFolder)) {
    Write-Error "Le dossier '$ModsFolder' n'existe pas."
    exit 1
}

$zipFiles = Get-ChildItem -Path $ModsFolder -Filter "*.zip" | Sort-Object Name

if ($zipFiles.Count -eq 0) {
    Write-Error "Aucun fichier .zip trouvé dans '$ModsFolder'."
    exit 1
}

$mods = @()
foreach ($file in $zipFiles) {
    Write-Host "Calcul du SHA-256 pour $($file.Name)..."
    $hash = (Get-FileHash -Path $file.FullName -Algorithm SHA256).Hash.ToLower()

    $mods += [ordered]@{
        file     = $file.Name
        version  = $Version
        sha256   = $hash
        size     = $file.Length
        required = $true
    }
}

$manifest = [ordered]@{
    name            = $Name
    version         = $Version
    gameVersion     = "FS25"
    serverAddress   = $ServerAddress
    serverPort      = $ServerPort
    statusUrl       = ""
    baseDownloadUrl = $BaseDownloadUrl
    changelog       = @()
    mods            = $mods
}

$manifest | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutFile -Encoding utf8

Write-Host ""
Write-Host "Manifeste généré : $OutFile ($($mods.Count) mods)"
Write-Host "Pensez à uploader les fichiers .zip dans une Release GitHub à l'URL indiquée par baseDownloadUrl."
