#!/bin/bash
# Script de demarrage pour le developpement local
# Lance le backend .NET et le frontend Svelte en parallele

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Couleurs
CYAN='\033[0;36m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
MAGENTA='\033[0;35m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

echo -e "${CYAN}=== Hytale Mod Lister - Dev Mode ===${NC}"
echo ""

# Fonction pour nettoyer les processus a la fermeture
cleanup() {
    echo ""
    echo -e "${YELLOW}Arret des serveurs...${NC}"
    if [ -n "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null || true
    fi
    if [ -n "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null || true
    fi
    exit 0
}

trap cleanup SIGINT SIGTERM

# Demarrer le backend
echo -e "${GREEN}[Backend] Demarrage sur http://localhost:5000...${NC}"
cd "$SCRIPT_DIR/backend/HytaleModLister.Api"
ADMIN_PASSWORD='test' dotnet run &
BACKEND_PID=$!

# Attendre un peu que le backend demarre
sleep 2

# Demarrer le frontend
echo -e "${BLUE}[Frontend] Demarrage sur http://localhost:5173...${NC}"
cd "$SCRIPT_DIR/frontend"
npm run dev &
FRONTEND_PID=$!

echo ""
echo -e "${YELLOW}Les deux applications tournent en arriere-plan.${NC}"
echo ""
echo -e "${CYAN}URLs:${NC}"
echo "  Frontend: http://localhost:5173"
echo "  Backend:  http://localhost:5000/api/mods"
echo ""
echo -e "${MAGENTA}Admin login: password = 'test'${NC}"
echo ""
echo -e "${GRAY}Appuyez sur Ctrl+C pour arreter les serveurs.${NC}"

# Attendre que les processus se terminent
wait
