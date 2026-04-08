Push-Location -Path .\ui

bun install
bun run build

Pop-Location

Remove-Item -Path .\artifacts -Recurse -Force -ErrorAction SilentlyContinue

dotnet clean -c Release
dotnet build -c Release
