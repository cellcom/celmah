# Celmah — Project Context

## What This Is

**Celmah** (Cellcom + Elmah, pronounced "sel-mah") is a fork of [Elmah.AspNetCore](https://github.com/jrsearles/Elmah.AspNetCore) retargeted to .NET 10 only, with updated dependencies and a full rename.

Target repo on GitHub: **https://github.com/cellcom/celmah** (org exists, repo not yet created)

## Fork Chain

```
Original ELMAH (Atif Aziz) — Apache-2.0
  └→ ElmahCore (Найченко et al.) — Apache-2.0
      └→ Elmah.AspNetCore (Joshua Searles) — Apache-2.0
          └→ Celmah (Cellcom Israel) — Apache-2.0 ✅
```

All Apache 2.0 — permissive, we can fork, rename, publish freely.
License file updated with Cellcom copyright added. `NOTICE` file documents the fork chain.

## Project Structure

```
src/Celmah/               Core library + embedded Vue SPA → NuGet: Celmah
src/Celmah.Common/         Shared types, ErrorLog base, options
src/Celmah.SqlServer/      SQL Server persistence → NuGet: Celmah.SqlServer
src/Celmah.Postgresql/     PostgreSQL persistence → NuGet: Celmah.Postgresql
src/Celmah.MySql/          MySQL persistence → NuGet: Celmah.MySql (not yet packed)
src/Celmah.Redis/          Redis persistence → NuGet: Celmah.Redis (not yet packed)
src/Celmah.Serilog/        Serilog sink → NuGet: Celmah.Serilog (not yet packed)
Tests/Celmah.Tests/        Unit tests
demos/Celmah.Demo/         Demo ASP.NET app
ui/                        Vue 3 SPA (Vite + Bun)
```

## Namespace & API Rename

Everything was renamed from `Elmah.AspNetCore.*` to `Celmah.*`:

| Old | New |
|---|---|
| `Elmah.AspNetCore` (namespace) | `Celmah` |
| `Elmah.AspNetCore.MsSql` | `Celmah.SqlServer` |
| `Elmah.AspNetCore.Postgresql` | `Celmah.Postgresql` |
| `Elmah.AspNetCore.MySql` | `Celmah.MySql` |
| `Elmah.AspNetCore.StackExchange.Redis` | `Celmah.Redis` |
| `Serilog.Sinks.Elmah.AspNetCore` | `Celmah.Serilog` |
| `UseElmah()` | `UseCelmah()` |
| `UseElmahMiddleware()` | `UseCelmahMiddleware()` |
| `MapElmah()` | `MapCelmah()` |
| `ElmahOptions` | `CelmahOptions` |
| `ElmahBuilder` | `CelmahBuilder` |
| `LogParamsToElmah()` | `LogParamsToCelmah()` |
| All other ElmahXxx types | CelmahXxx |

Default route prefix changed from `/elmah` to `/celmah`.

## NuGet Packages

Package IDs are all new (not conflicting with upstream on nuget.org):

| Package | Status |
|---|---|
| `Celmah` | ✅ Building & packing |
| `Celmah.SqlServer` | ✅ Building & packing |
| `Celmah.Postgresql` | ✅ Building & packing |
| `Celmah.MySql` | ⬜ Not yet in build script |
| `Celmah.Redis` | ⬜ Not yet in build script |
| `Celmah.Serilog` | ⬜ Not yet in build script |

Local feed: `/mnt/c/git/nuget/Celmah` (or `/mnt/c/git/nuget/Celmah` after rename)

## Build Scripts

| Script | Purpose |
|---|---|
| `./build-local.sh` | Build SPA + pack NuGet (fast, no publish) |
| `./build-and-publish-local.sh` | Build + pack + push to local feed |

Prerequisites: .NET 10 SDK, Bun

## SPA Base Path Strategy

See `docs/SPA-BASE-PATH.md` for full details and rollback instructions.

Current approach (no magic strings):
1. Vite builds with `base: './'` (relative paths, no custom plugin)
2. Backend (`ErrorResourceHandler.cs`) injects `<meta name="celmah-root" content="...">` into `<head>`
3. Backend rewrites `src="./` → `src="{celmahRoot}/"` for absolute asset paths (needed because client-side routing breaks relative paths)
4. JS reads the meta tag for API base URL and Vue Router history base
5. `GetCelmahRelativeRoot()` includes `PathBase` → reverse proxy support works

## Key Files Modified/Created

| File | Purpose |
|---|---|
| `Directory.Build.props` | Moved from `temp/` to root — NuGet metadata, symbols, UseArtifactsOutput |
| `nuget.config` | Local feed at `/mnt/c/git/nuget/Celmah` |
| `Celmah.sln` | Renamed from `Elmah.AspNetCore.sln` |
| `README.md` | Full rewrite — Celmah branding, migration guide, build instructions |
| `NOTICE` | Apache 2.0 derivative work attribution |
| `LICENSE` | Added Cellcom copyright line |
| `FORK-GUIDANCE.md` | Licensing analysis, NuGet publishing guidance |
| `docs/SPA-BASE-PATH.md` | SPA base path strategy doc + rollback instructions |

## Remaining Work

- [ ] Create repo on GitHub (`cellcom/celmah`)
- [ ] Add remaining packages to build scripts (MySql, Redis, Serilog)
- [ ] Test with a real ASP.NET app consuming the local NuGet packages
- [ ] Test behind reverse proxy (PathBase scenario)
- [ ] Decide on version number for nuget.org (1.0.0 vs 10.0.0)
- [ ] Publish to nuget.org when stable (needs API key)
- [ ] Clean up `temp/` directory (stale Directory.Build.props etc.)
- [ ] The `FORK-GUIDANCE.md` still references old Elmah.AspNetCore in places — update or remove
