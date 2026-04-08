#!/usr/bin/env bash
#
# build-local.sh
# Builds the Vue SPA and packs NuGet packages (no publish).
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
CONFIG="Release"

echo "========================================"
echo " Celmah – Build"
echo "========================================"

# --- 1. Build the Vue SPA ---
echo ">>> Building Vue SPA..."
cd "$REPO_ROOT/ui"
bun install
bun run build
echo "    SPA built → src/Celmah/wwwroot/"

# --- 2. Pack NuGet packages ---
echo ">>> Packing NuGet packages..."
cd "$REPO_ROOT"
rm -rf artifacts/package

PACKAGES=(
  "src/Celmah/Celmah.csproj"
  "src/Celmah.SqlServer/Celmah.SqlServer.csproj"
  "src/Celmah.Postgresql/Celmah.Postgresql.csproj"
)

for proj in "${PACKAGES[@]}"; do
  echo "    dotnet pack $proj -c $CONFIG"
  dotnet pack "$proj" -c "$CONFIG"
done

echo ""
echo "========================================"
echo " Done!"
echo "========================================"
