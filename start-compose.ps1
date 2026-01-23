# HytaleModLister - Script de demarrage via Docker Compose
# Usage: .\start-compose.ps1 [-Build] [-Stop]

param(
    [switch]$Build,
    [switch]$Stop
)

$ErrorActionPreference = "Stop"

Write-Host "=== HytaleModLister ===" -ForegroundColor Cyan

# Verifier que Docker est installe
if (-not (Get-Command "docker" -ErrorAction SilentlyContinue)) {
    Write-Host "Erreur: Docker n'est pas installe ou pas dans le PATH" -ForegroundColor Red
    Write-Host "Installez Docker Desktop: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

# Verifier que Docker est en cours d'execution
$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur: Docker n'est pas en cours d'execution" -ForegroundColor Red
    Write-Host "Demarrez Docker Desktop et reessayez" -ForegroundColor Yellow
    exit 1
}

Write-Host "Docker OK" -ForegroundColor Green

# Mode stop
if ($Stop) {
    Write-Host "Arret des services..." -ForegroundColor Yellow
    docker compose down
    Write-Host "Services arretes" -ForegroundColor Green
    exit 0
}

# Creer le fichier .env s'il n'existe pas
$envFile = Join-Path $PSScriptRoot ".env"
if (-not (Test-Path $envFile)) {
    Write-Host "Creation du fichier .env..." -ForegroundColor Yellow

    $apiKey = Read-Host "Entrez votre cle API CurseForge (optionnel, appuyez sur Entree pour ignorer)"

    # Echapper les $ pour Docker Compose ($ -> $$)
    $apiKey = $apiKey -replace '\$', '$$$$'

    @"
# Configuration HytaleModLister
# Cle API CurseForge (obtenir sur https://console.curseforge.com/)
CURSEFORGE_API_KEY=$apiKey
"@ | Out-File -FilePath $envFile -Encoding utf8

    Write-Host "Fichier .env cree" -ForegroundColor Green
} else {
    Write-Host "Fichier .env existant" -ForegroundColor Green
}

# Creer le dossier mods s'il n'existe pas
$modsDir = Join-Path $PSScriptRoot "mods"
if (-not (Test-Path $modsDir)) {
    Write-Host "Creation du dossier mods..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $modsDir | Out-Null
    Write-Host "Dossier mods cree" -ForegroundColor Green
} else {
    Write-Host "Dossier mods existant" -ForegroundColor Green
}

# Lancer docker compose
Write-Host ""
if ($Build) {
    Write-Host "Construction et demarrage des services..." -ForegroundColor Cyan
    docker compose up --build -d
} else {
    Write-Host "Demarrage des services..." -ForegroundColor Cyan
    docker compose up -d
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors du demarrage des services" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Services demarres ===" -ForegroundColor Green
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Backend:  http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Commandes utiles:" -ForegroundColor Yellow
Write-Host "  .\start-compose.ps1 -Stop     # Arreter les services"
Write-Host "  .\start-compose.ps1 -Build    # Reconstruire et demarrer"
Write-Host "  docker compose logs           # Voir les logs"
