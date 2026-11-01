#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Build, pack, and optionally publish Celmah NuGet packages.

.EXAMPLE
  ./build.ps1                       # Build all packages (core + extensions)
  ./build.ps1 -Core                 # Build only Celmah + Celmah.Common
  ./build.ps1 -Push                 # Build all and publish to the local feed
  ./build.ps1 -Core -Push           # Build only core and publish to the local feed
  ./build.ps1 -Push -Feed D:\feed   # Build all and publish to a custom feed
  ./build.ps1 -SkipSpa              # Skip the Vue SPA build
  ./build.ps1 -NoPush               # Build only, publish nowhere

.NOTES
  When CELMAH_NUGET_SOURCE and CELMAH_NUGET_API_KEY are set, every build also publishes
  to that NuGet server (versions already there are skipped). -NoPush turns this off.
  -Source / -ApiKey override the environment variables.
#>
[CmdletBinding()]
param(
    [switch]$Core,                   # Build only Celmah + Celmah.Common (no extensions)
    [switch]$Push,                   # Publish to the local NuGet feed
    [switch]$NoPush,                 # Publish nowhere (overrides -Push and the NuGet server)
    [string]$Feed = $(if ($IsWindows) { 'C:\git\nuget\Celmah' } else { '/mnt/c/git/nuget/Celmah' }),
    [string]$Source = $env:CELMAH_NUGET_SOURCE,   # NuGet server to publish to
    [string]$ApiKey = $env:CELMAH_NUGET_API_KEY,  # API key for -Source
    [switch]$SkipSpa                 # Skip the Vue SPA build
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true   # fail on non-zero exit from bun / dotnet

$RepoRoot = $PSScriptRoot
$Config   = 'Release'

# --- Package lists ---
$AllPackages = @(
    'Celmah.Common/Celmah.Common.csproj'
    'Celmah/Celmah.csproj'
    'Celmah.SqlServer/Celmah.SqlServer.csproj'
    'Celmah.Postgresql/Celmah.Postgresql.csproj'
    'Celmah.MySql/Celmah.MySql.csproj'
    'Celmah.Redis/Celmah.Redis.csproj'
    'Celmah.Serilog/Celmah.Serilog.csproj'
)
$CorePackages = @(
    'Celmah.Common/Celmah.Common.csproj'
    'Celmah/Celmah.csproj'
)
$Packages = if ($Core) { $CorePackages } else { $AllPackages }
$Scope    = if ($Core) { 'core' } else { 'all' }

if ($NoPush) { $Push = $false }
$PushServer = -not $NoPush -and $Source -and $ApiKey
if (-not $NoPush -and [bool]$Source -ne [bool]$ApiKey) {
    Write-Warning 'Set both CELMAH_NUGET_SOURCE and CELMAH_NUGET_API_KEY (or -Source and -ApiKey) to publish to a NuGet server.'
}

# --- Header ---
Write-Host '========================================'
Write-Host ' Celmah – Build'
Write-Host '========================================'
Write-Host "Scope    : $Scope"
Write-Host "Packages : $($Packages.Count)"
Write-Host "Push     : $Push"
if ($Push) { Write-Host "Feed     : $Feed" }
if ($PushServer) { Write-Host "Server   : $Source" }
Write-Host ''

# --- 1. Build the Vue SPA ---
if (-not $SkipSpa) {
    Write-Host '>>> Building Vue SPA...'
    Push-Location (Join-Path $RepoRoot 'Celmah/ui')
    try {
        bun install
        bun run build
    } finally { Pop-Location }
    Write-Host '    SPA built → Celmah/wwwroot/'
    Write-Host ''
}

# --- 2. Pack NuGet packages ---
Write-Host '>>> Packing NuGet packages...'
$PackageDir = Join-Path $RepoRoot 'artifacts/package/release'
Remove-Item (Join-Path $RepoRoot 'artifacts/package') -Recurse -Force -ErrorAction SilentlyContinue

foreach ($proj in $Packages) {
    Write-Host "    dotnet pack $proj -c $Config"
    dotnet pack (Join-Path $RepoRoot $proj) -c $Config
}
Write-Host ''

# --- 3. Publish (optional) ---
if ($Push) {
    Write-Host ">>> Publishing to local feed: $Feed"
    New-Item -ItemType Directory -Force -Path $Feed | Out-Null

    foreach ($nupkg in Get-ChildItem $PackageDir -Filter *.nupkg) {
        Write-Host "    pushing $($nupkg.Name)"
        dotnet nuget push $nupkg.FullName --source $Feed
    }
    Write-Host ''
}

if ($PushServer) {
    Write-Host ">>> Publishing to NuGet server: $Source"
    foreach ($nupkg in Get-ChildItem $PackageDir -Filter *.nupkg) {
        Write-Host "    pushing $($nupkg.Name)"
        dotnet nuget push $nupkg.FullName --source $Source --api-key $ApiKey --skip-duplicate
    }
    Write-Host ''
}

# --- Summary ---
Write-Host '========================================'
Write-Host ' Done!'
Write-Host ''
Write-Host ' Generated packages:'
Get-ChildItem $PackageDir -Filter *.nupkg -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "   $($_.Name)" }
Write-Host '========================================'

if ($Push) {
    Write-Host ''
    Write-Host 'To consume in another project, add this nuget.config:'
    Write-Host ''
    Write-Host @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="CelmahLocal" value="$Feed" />
  </packageSources>
</configuration>
"@
    Write-Host ''
    Write-Host 'Then run:'
    foreach ($proj in $Packages) {
        Write-Host "  dotnet add package $([IO.Path]::GetFileNameWithoutExtension($proj))"
    }
}
