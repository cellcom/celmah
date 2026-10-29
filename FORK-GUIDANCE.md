# Fork, licensing & publishing

## Fork chain

```
ELMAH (Atif Aziz et al.)                       https://elmah.github.io/
  └─ ElmahCore (Андрей Найченко et al.)         https://github.com/ElmahCore/ElmahCore
      └─ Elmah.AspNetCore (Joshua Searles)      https://github.com/jrsearles/Elmah.AspNetCore
          └─ Celmah (Cellcom Israel)            https://github.com/cellcom/celmah
```

Every generation is licensed under the **Apache License 2.0**. Celmah stays Apache 2.0.

## Apache 2.0 obligations

| Requirement | Where it is met |
|---|---|
| Include the license | `LICENSE` (Apache 2.0, with the Cellcom copyright line added) |
| Keep existing copyright notices | Original headers (e.g. `Copyright (c) Atif Aziz`) are kept in the source files they came with |
| State significant changes | `NOTICE` lists the upstream projects and the modifications |
| Ship `NOTICE` with redistributions | `NOTICE` is at the repository root |

When changing these files, keep upstream notices intact and add to them rather than replacing them.

## Relationship to upstream

- Celmah is a renamed, .NET 10-only fork of Elmah.AspNetCore. Namespaces, types, extension methods (`UseCelmah`, `MapCelmah`, …), package IDs and the default route (`/celmah`) all differ from upstream, so both can be installed side by side.
- The upstream repository is kept as the `upstream` git remote. Upstream fixes are merged manually; expect conflicts in renamed files.

## Packages

| Package | Contents |
|---|---|
| `Celmah` | Middleware and UI |
| `Celmah.Common` | Shared types, `ErrorLog` base class, in-memory and XML file logs |
| `Celmah.SqlServer`, `Celmah.Postgresql`, `Celmah.MySql`, `Celmah.Redis` | Persistence providers |
| `Celmah.Serilog` | Serilog sink |

Package metadata (authors, copyright, license expression, repository URLs) is set once in `Directory.Build.props`. Versions come from Nerdbank.GitVersioning (`version.json` per project); bump them together with `./bump-version.ps1`.

## Publishing to nuget.org

Not published yet. The `Celmah*` package IDs are not taken on nuget.org.

To enable publishing:

1. Create an API key on nuget.org scoped to `Celmah*`.
2. Add it as the `NUGET_APIKEY` repository secret on GitHub.
3. In `.github/workflows/ci.yml`, remove `if: false` from the `publish` job. Pushes to `main` will then publish the packages built by CI (`--skip-duplicate` ignores versions that already exist).

Until then, packages are built locally with `./build.ps1` (optionally `-Push` to a local feed) and are available as CI build artifacts.
