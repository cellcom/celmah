# Guidance: Forking, Publishing & Licensing

## The Fork Chain

```
Original ELMAH (Atif Aziz et al.)        — Apache-2.0
  └→ ElmahCore (ElmahCore org, Андрей Найченко et al.) — Apache-2.0
      └→ Celmah (Joshua Searles)              — Apache-2.0
          └→ Your fork (retargeted to .NET 10)           — Apache-2.0 ✅
```

All three generations use **Apache License 2.0**. This is a permissive license that:
- ✅ Allows forking and modification
- ✅ Allows publishing derivative packages (NuGet, etc.)
- ✅ Allows changing the target framework
- ✅ Allows keeping your changes closed-source or open-source

## Apache 2.0 Obligations

If you publish your fork (GitHub or NuGet), you **must**:

1. **Include a copy of the Apache 2.0 license** — ✅ Already present as `LICENSE`
2. **Retain all copyright notices in source files** — ✅ Files like `StackTraceParser.cs`, `EmailOptions.cs` etc. still have `Copyright (c) Atif Aziz` headers
3. **State if you changed files** — You should add a `NOTICE` file documenting modifications
4. **Include the `NOTICE` file** if the original had one — It didn't

That's it. No copyleft, no requirement to publish source, no fee.

## Recommended: Create a NOTICE File

Apache 2.0 Section 4(b) says modified files should carry prominent notices. A `NOTICE` file at the repo root covers this cleanly:

```
Celmah (.NET 10 fork)
Copyright 2024-2026 Your Name

This project is a fork of Celmah by Joshua Searles
https://github.com/jrsearles/Celmah

Which is itself a fork of ElmahCore
https://github.com/ElmahCore/ElmahCore

Which is based on the original ELMAH project by Atif Aziz
https://elmah.github.io/

Licensed under the Apache License, Version 2.0.

Modifications from upstream:
- Retargeted to .NET 10 only (removed multi-targeting)
- Updated NuGet package dependencies
- Added local build/publish tooling
```

## Should You Fork on GitHub?

**Yes, absolutely.** Here's the recommended approach:

### Option A: Public Fork via GitHub "Fork" button (recommended)
- Go to https://github.com/jrsearles/Celmah
- Click **"Fork"**
- This preserves the fork relationship and shows up as a fork on the upstream repo
- Push your changes to your fork's `main` branch
- Pro: GitHub shows the connection, easy to sync upstream changes
- Con: Can't easily publish under a different NuGet package ID on nuget.org if you want to

### Option B: Standalone public repo
- Create a new repo like `yourname/Celmah.Net10`
- Push the code there (removing the `.git` history or keeping it)
- Pro: Clean identity, doesn't look like a "secondary" fork
- Con: Loses the GitHub fork connection

### What to update before pushing

1. **`Directory.Build.props`** — Update `RepositoryUrl` and `PackageProjectUrl` to your repo
2. **`README.md`** — Already updated ✅
3. **`LICENSE`** — Keep as-is, but update the copyright line to add yours:

   ```
   Copyright 2018 ElmahCore
   Copyright 2024-2026 Your Name

   Licensed under the Apache License, Version 2.0 ...
   ```
4. **Add `NOTICE` file** (see above)

## Publishing to NuGet.org

### Current state on nuget.org

These packages already exist under the upstream author's account:

| Package                              | Latest Stable | TFM    |
|--------------------------------------|---------------|--------|
| Celmah                     | 1.0.3         | net6.0 |
| Celmah.SqlServer               | 1.0.1         | net6.0 |
| Celmah.Postgresql          | 1.0.1         | net6.0 |
| Celmah.MySql               | 1.0.1         | net6.0 |
| Celmah.Redis | 1.1.1         | net6.0 |
| Celmah.Serilog       | 1.0.2         | net6.0 |

**You cannot push to these package IDs** — they're owned by the upstream author's NuGet account.

### Options for publishing to nuget.org

#### Option 1: Use different package IDs (easiest, do this)
Rename the packages to clearly differentiate. Examples:
- `Celmah.Net10`
- `Celmah.SqlServer.Net10`
- `Celmah.Postgresql.Net10`

Just update `<PackageId>` in each `.csproj` and `Directory.Build.props`.

#### Option 2: Ask upstream to transfer or add you as owner
Contact Joshua Searles via GitHub issues and ask to be added as a co-owner on the NuGet packages.
This is unlikely unless you contribute back.

#### Option 3: Contribute upstream
Open a PR to the upstream repo adding .NET 10 targeting. If merged, the upstream packages get updated.
However, upstream targets .NET 6+, so they may not want to drop multi-targeting.

### Steps to publish to nuget.org

1. **Create a NuGet.org account** at https://www.nuget.org/
2. **Generate an API key** at https://www.nuget.org/account/apikeys
3. **Update package metadata** (ID, repo URLs, etc.)
4. **Push:**

```bash
dotnet nuget push artifacts/package/release/YourPackage.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Version numbering

Since these would be new package IDs, start at `1.0.0` or `10.0.0` (to signal .NET 10).
If you keep `1.0.0`, your `version.json` and `Directory.Build.props` already handle this.

## Quick Decision Matrix

| Question                                    | Recommendation                          |
|---------------------------------------------|-----------------------------------------|
| Fork on GitHub?                             | **Yes** — use GitHub "Fork" button      |
| What license?                               | **Keep Apache 2.0** (required)          |
| Can I publish to NuGet?                     | **Yes** but with different Package IDs  |
| Suggested Package IDs?                      | Add `.Net10` suffix                     |
| Need to update LICENSE?                     | Add your copyright, keep Apache 2.0     |
| Need NOTICE file?                           | **Yes** — documents the fork chain      |
| Must I open-source my changes?              | **No** — Apache 2.0 is permissive       |
