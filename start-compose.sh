#!/bin/bash
# HytaleModLister - Script de demarrage via Docker Compose
# Usage: ./start-compose.sh [--build] [--stop]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

BUILD=false
STOP=false

# Parser les arguments
for arg in "$@"; do
    case $arg in
        --build|-b)
            BUILD=true
            ;;
        --stop|-s)
            STOP=true
            ;;
        --help|-h)
            echo "Usage: $0 [--build] [--stop]"
            echo "  --build, -b  Reconstruire les images avant de demarrer"
            echo "  --stop, -s   Arreter les services"
            exit 0
            ;;
    esac
done

echo "=== HytaleModLister ==="

# Verifier que Docker est installe
if ! command -v docker &> /dev/null; then
    echo "Erreur: Docker n'est pas installe ou pas dans le PATH"
    echo "Installez Docker: https://docs.docker.com/get-docker/"
    exit 1
fi

# Verifier que Docker est en cours d'execution
if ! docker info &> /dev/null; then
    echo "Erreur: Docker n'est pas en cours d'execution"
    echo "Demarrez le daemon Docker et reessayez"
    exit 1
fi

echo "Docker OK"

# Mode stop
if [ "$STOP" = true ]; then
    echo "Arret des services..."
    docker compose down
    echo "Services arretes"
    exit 0
fi

# Creer le fichier .env s'il n'existe pas
if [ ! -f ".env" ]; then
    echo "Creation du fichier .env..."

    read -p "Entrez votre cle API CurseForge (optionnel, appuyez sur Entree pour ignorer): " API_KEY

    # Echapper les $ pour Docker Compose ($ -> $$)
    API_KEY="${API_KEY//\$/\$\$}"

    cat > .env << EOF
# Configuration HytaleModLister
# Cle API CurseForge (obtenir sur https://console.curseforge.com/)
CURSEFORGE_API_KEY=$API_KEY
EOF

    echo "Fichier .env cree"
else
    echo "Fichier .env existant"
fi

# Creer le dossier mods s'il n'existe pas
if [ ! -d "mods" ]; then
    echo "Creation du dossier mods..."
    mkdir -p mods
    echo "Dossier mods cree"
else
    echo "Dossier mods existant"
fi

# Lancer docker compose
echo ""
if [ "$BUILD" = true ]; then
    echo "Construction et demarrage des services..."
    docker compose up --build -d
else
    echo "Demarrage des services..."
    docker compose up -d
fi

echo ""
echo "=== Services demarres ==="
echo "Frontend: http://localhost:3000"
echo "Backend:  http://localhost:5000"
echo ""
echo "Commandes utiles:"
echo "  ./start-compose.sh --stop     # Arreter les services"
echo "  ./start-compose.sh --build    # Reconstruire et demarrer"
echo "  docker compose logs           # Voir les logs"
