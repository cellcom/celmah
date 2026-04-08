#!/usr/bin/env bash
#
# build-and-publish-local.sh
# Builds the Vue SPA, packs Celmah NuGet packages, and publishes them to a local feed.
#
# Usage:
#   ./build-and-publish-local.sh                  # default feed: /mnt/c/git/nuget/Celmah
#   ./build-and-publish-local.sh /path/to/feed    # custom feed path
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
LOCAL_FEED="${1:-/mnt/c/git/nuget/Celmah}"
CONFIG="Release"

echo "========================================"
echo " Celmah – Local NuGet Build"
echo "========================================"
echo "Repo root : $REPO_ROOT"
echo "Local feed: $LOCAL_FEED"
echo ""

# --- 1. Build the Vue SPA ---
echo ">>> Building Vue SPA..."
cd "$REPO_ROOT/ui"
bun install
bun run build
echo "    SPA built → src/Celmah/wwwroot/"
echo ""

# --- 2. Pack NuGet packages ---
echo ">>> Packing NuGet packages..."
cd "$REPO_ROOT"
rm -rf artifacts/package

PACKAGES=(
  "src/Celmah/Celmah.csproj"
  "src/Celmah.SqlServer/Celmah.SqlServer.csproj"
  "src/Celmah.Postgresql/Celmah.Postgresql.csproj"
  "src/Celmah.MySql/Celmah.MySql.csproj"
  "src/Celmah.Redis/Celmah.Redis.csproj"
  "src/Celmah.Serilog/Celmah.Serilog.csproj"
)

for proj in "${PACKAGES[@]}"; do
  echo "    dotnet pack $proj -c $CONFIG"
  dotnet pack "$proj" -c "$CONFIG"
done
echo ""

# --- 3. Publish to local feed ---
echo ">>> Publishing to local feed: $LOCAL_FEED"
mkdir -p "$LOCAL_FEED"

for nupkg in "$REPO_ROOT"/artifacts/package/release/*.nupkg; do
  # Skip symbol packages
  case "$nupkg" in *.snupkg) continue ;; esac
  echo "    pushing $(basename "$nupkg")"
  dotnet nuget push "$nupkg" --source "$LOCAL_FEED"
done

echo ""
echo "========================================"
echo " Done! Packages published to:"
echo "   $LOCAL_FEED"
echo ""
echo " Generated packages:"
ls -1 "$LOCAL_FEED"/*.nupkg 2>/dev/null | xargs -n1 basename
echo "========================================"
echo ""
echo "To consume in another project, add this nuget.config:"
echo ""
cat << 'NUGETCONFIG'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="CelmahLocal" value="/mnt/c/git/nuget/Celmah" />
  </packageSources>
</configuration>
NUGETCONFIG
echo ""
echo "Then run:"
echo "  dotnet add package Celmah"
echo "  dotnet add package Celmah.SqlServer"
echo "  dotnet add package Celmah.Postgresql"
echo "  dotnet add package Celmah.MySql"
echo "  dotnet add package Celmah.Redis"
echo "  dotnet add package Celmah.Serilog"
