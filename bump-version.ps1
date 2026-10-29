#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Bump the Celmah version in every version.json.

.DESCRIPTION
  Nerdbank.GitVersioning reads the version.json closest to each project, and
  every project has its own (plus one at the repo root) — 8 files in total. They
  all have to move together, otherwise packages keep building at the old version.
  Line endings of the files are preserved.

.EXAMPLE
  ./bump-version.ps1 1.4              # bump to 1.4 and commit
  ./bump-version.ps1 1.4 -NoCommit    # bump without committing
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidatePattern('^\d+\.\d+(\.\d+)?$', ErrorMessage = "Invalid version '{0}' (expected major.minor or major.minor.build, e.g. 1.4)")]
    [string]$Version,
    [switch]$NoCommit
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$RepoRoot = $PSScriptRoot

# Root + every project that carries its own version.json.
$Files = @(
    'version.json'
    'Celmah/version.json'
    'Celmah.Common/version.json'
    'Celmah.SqlServer/version.json'
    'Celmah.Postgresql/version.json'
    'Celmah.MySql/version.json'
    'Celmah.Redis/version.json'
    'Celmah.Serilog/version.json'
)

foreach ($f in $Files) {
    if (-not (Test-Path (Join-Path $RepoRoot $f))) {
        throw "Missing $f — update the `$Files list in $PSCommandPath"
    }
}

Write-Host "Bumping $($Files.Count) version.json files to ${Version}:"
foreach ($f in $Files) {
    $path = Join-Path $RepoRoot $f
    $text = [IO.File]::ReadAllText($path)
    $text = [regex]::Replace($text, '("version"\s*:\s*")[^"]+', "`${1}$Version")
    [IO.File]::WriteAllText($path, $text)   # no BOM, line endings untouched
    Write-Host "  $f"
}

if (-not $NoCommit) {
    git -C $RepoRoot add -- $Files
    git -C $RepoRoot commit -m "bump version to $Version" | Out-Null
    Write-Host "Committed: bump version to $Version"
    Write-Host 'Next: ./build.ps1 -Push'
} else {
    Write-Host 'Not committing (-NoCommit). Review with: git diff'
}
