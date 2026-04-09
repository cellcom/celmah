#!/usr/bin/env bash
#
# build.sh — Build, pack, and optionally publish Celmah NuGet packages.
#
# Usage:
#   ./build.sh                          Build all packages (core + extensions)
#   ./build.sh --core                   Build only Celmah + Celmah.Common
#   ./build.sh --push                   Build all and publish to local feed
#   ./build.sh --core --push            Build only core and publish to local feed
#   ./build.sh --push /path/to/feed     Build all and publish to custom feed
#   ./build.sh --skip-spa               Skip the Vue SPA build
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
CONFIG="Release"
LOCAL_FEED="/mnt/c/git/nuget/Celmah"
PUSH=false
CORE_ONLY=false
SKIP_SPA=false

# --- Parse arguments ---
FEED_ARG=""
for arg in "$@"; do
  case "$arg" in
    --core)        CORE_ONLY=true ;;
    --push)        PUSH=true ;;
    --skip-spa)    SKIP_SPA=true ;;
    -h|--help)
      echo "Usage: $0 [OPTIONS] [--push [FEED_PATH]]"
      echo ""
      echo "Options:"
      echo "  --core        Build only Celmah + Celmah.Common (no extensions)"
      echo "  --push [PATH] Build and publish to local NuGet feed (default: $LOCAL_FEED)"
      echo "  --skip-spa    Skip Vue SPA build"
      echo "  -h, --help    Show this help"
      exit 0
      ;;
    *)
      # Treat as feed path if --push was already seen
      if [ "$PUSH" = true ]; then
        LOCAL_FEED="$arg"
      else
        echo "Unknown argument: $arg"
        exit 1
      fi
      ;;
  esac
done

# --- Package lists ---
ALL_PACKAGES=(
  "Celmah.Common/Celmah.Common.csproj"
  "Celmah/Celmah.csproj"
  "Celmah.SqlServer/Celmah.SqlServer.csproj"
  "Celmah.Postgresql/Celmah.Postgresql.csproj"
  "Celmah.MySql/Celmah.MySql.csproj"
  "Celmah.Redis/Celmah.Redis.csproj"
  "Celmah.Serilog/Celmah.Serilog.csproj"
)

CORE_PACKAGES=(
  "Celmah.Common/Celmah.Common.csproj"
  "Celmah/Celmah.csproj"
)

if [ "$CORE_ONLY" = true ]; then
  PACKAGES=("${CORE_PACKAGES[@]}")
  SCOPE="core"
else
  PACKAGES=("${ALL_PACKAGES[@]}")
  SCOPE="all"
fi

# --- Header ---
echo "========================================"
echo " Celmah – Build"
echo "========================================"
echo "Scope    : $SCOPE"
echo "Packages : ${#PACKAGES[@]}"
echo "Push     : $PUSH"
if [ "$PUSH" = true ]; then
  echo "Feed     : $LOCAL_FEED"
fi
echo ""

# --- 1. Build the Vue SPA ---
if [ "$SKIP_SPA" = false ]; then
  echo ">>> Building Vue SPA..."
  cd "$REPO_ROOT/Celmah/ui"
  bun install
  bun run build
  echo "    SPA built → Celmah/wwwroot/"
  echo ""
fi

# --- 2. Pack NuGet packages ---
echo ">>> Packing NuGet packages..."
cd "$REPO_ROOT"
rm -rf artifacts/package

for proj in "${PACKAGES[@]}"; do
  echo "    dotnet pack $proj -c $CONFIG"
  dotnet pack "$proj" -c "$CONFIG"
done
echo ""

# --- 3. Publish (optional) ---
if [ "$PUSH" = true ]; then
  echo ">>> Publishing to local feed: $LOCAL_FEED"
  mkdir -p "$LOCAL_FEED"

  for nupkg in "$REPO_ROOT"/artifacts/package/release/*.nupkg; do
    case "$nupkg" in *.snupkg) continue ;; esac
    echo "    pushing $(basename "$nupkg")"
    dotnet nuget push "$nupkg" --source "$LOCAL_FEED"
  done
  echo ""
fi

# --- Summary ---
echo "========================================"
echo " Done!"
echo ""
echo " Generated packages:"
ls -1 "$REPO_ROOT"/artifacts/package/release/*.nupkg 2>/dev/null | xargs -n1 basename
echo "========================================"

if [ "$PUSH" = true ]; then
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
  echo "  dotnet add package Celmah.Common"
  echo "  dotnet add package Celmah"
  if [ "$CORE_ONLY" = false ]; then
    echo "  dotnet add package Celmah.SqlServer"
    echo "  dotnet add package Celmah.Postgresql"
    echo "  dotnet add package Celmah.MySql"
    echo "  dotnet add package Celmah.Redis"
    echo "  dotnet add package Celmah.Serilog"
  fi
fi
