#!/usr/bin/env bash
#
# bump-version.sh — bump the Celmah version in every version.json.
#
# Nerdbank.GitVersioning reads the version.json closest to each project, and
# every project has its own (plus one at the repo root) — 8 files in total. They
# all have to move together, otherwise packages keep building at the old version.
#
# Usage:
#   ./bump-version.sh 1.4              bump to 1.4 and commit
#   ./bump-version.sh 1.4 --no-commit  bump without committing
#
set -euo pipefail

if [ $# -lt 1 ] || [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
  echo "Usage: $0 <version> [--no-commit]"
  echo "  e.g. $0 1.4"
  exit 1
fi

VERSION="$1"
COMMIT=true
[ "${2:-}" = "--no-commit" ] && COMMIT=false

if ! echo "$VERSION" | grep -qE '^[0-9]+\.[0-9]+(\.[0-9]+)?$'; then
  echo "Invalid version '$VERSION' (expected major.minor or major.minor.build, e.g. 1.4)" >&2
  exit 1
fi

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

# Root + every project that carries its own version.json.
FILES=(
  "$REPO_ROOT/version.json"
  "$REPO_ROOT/Celmah/version.json"
  "$REPO_ROOT/Celmah.Common/version.json"
  "$REPO_ROOT/Celmah.SqlServer/version.json"
  "$REPO_ROOT/Celmah.Postgresql/version.json"
  "$REPO_ROOT/Celmah.MySql/version.json"
  "$REPO_ROOT/Celmah.Redis/version.json"
  "$REPO_ROOT/Celmah.Serilog/version.json"
)

for f in "${FILES[@]}"; do
  if [ ! -f "$f" ]; then
    echo "Missing $f — update the FILES list in $0" >&2
    exit 1
  fi
done

echo "Bumping ${#FILES[@]} version.json files to $VERSION:"
for f in "${FILES[@]}"; do
  sed -i -E "s/(\"version\"[[:space:]]*:[[:space:]]*\")[^\"]+/\1$VERSION/" "$f"
  echo "  ${f#$REPO_ROOT/}"
done

if [ "$COMMIT" = true ]; then
  cd "$REPO_ROOT"
  git add "${FILES[@]}"
  git commit -m "bump version to $VERSION" >/dev/null
  echo "Committed: bump version to $VERSION"
  echo "Next: ./build.sh --push"
else
  echo "Not committing (--no-commit). Review with: git diff"
fi
