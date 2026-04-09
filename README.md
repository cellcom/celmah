[![License](https://img.shields.io/github/license/cellcom/celmah)](LICENSE)

<!-- #intro -->
# Celmah

**C**ellcom **Elmah** — ELMAH (Error Logging Middleware and Handlers) for ASP.NET Core, targeting .NET 10.

> **This is a fork of [jrsearles/Elmah.AspNetCore](https://github.com/jrsearles/Elmah.AspNetCore)**,
> retargeted to **.NET 10** with updated NuGet package dependencies.

Features include:

- Logging of unhandled exceptions
- Friendly UI to view captured errors along with contextual information
- Hooks to include handled exceptions and additional contextual information
- Various methods to [persist error logs](#error-persistence)
- Supports [securing UI](#restrict-access-to-the-celmah-ui) via built-in ASP.NET Core functionality
- [Notifications of errors](#using-notifiers) through email or custom notifiers
- Integration with `Microsoft.Extensions.Logging` to capture logs made during a request
- Targets **.NET 10** only
<!-- #intro -->
![alt text](https://github.com/ElmahCore/ElmahCore/raw/master/images/elmah-new-ui.png)

> This is a fork of [ElmahCore](https://github.com/ElmahCore/ElmahCore) which is itself a fork of the original [Elmah](https://elmah.github.io/) library. Credit goes to the owners and contributors of those libraries. This fork retargets to .NET 10 and updates all NuGet package references.

<!-- #usage -->
## Basic usage

**First**, install the _Celmah_ NuGet package into your app.

```shell
dotnet add package Celmah
```

**Next**, in your application's _Program.cs_ file, configure Celmah:

```csharp
using Celmah;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseCelmah(); // <- Add this to configure Celmah

var app = builder.Build();

app.UseExceptionHandler();
app.UseCelmahMiddleware(); // <- Add this to register middleware

app.MapCelmah(); // <- Add this to register Celmah endpoints
```

`builder.Host.UseCelmah()` registers and configures the Celmah services. An overload which accepts an action is available to modify the configuration.

`app.UseCelmahMiddleware()` registers the middleware used by Celmah to start capturing errors and contextual information. Only middleware registered after the Celmah middleware will be included in the error capturing. It is recommended that this is included before most other middleware. For best results, call after the built-in `UseExceptionHandler()`.

`app.MapCelmah()` registers the routes used to serve content for the Celmah UI. By default these will be under `/celmah`, but the method includes an overload which allows overriding the root path.
<!-- #usage -->
## Celmah Options

| Option                | Type                           | Default                                 | Description                                                                |
| --------------------- | ------------------------------ | --------------------------------------- | -------------------------------------------------------------------------- |
| ApplicationName       | string                         | ApplicationName from [`IHostEnvironment`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostenvironment) | Application name captured in error log |
| Filters               | IErrorFilter[]                 | empty                                   | A collection of [`IErrorFilter`](#using-filters) instances
| FiltersConfig         | string                         | `null`                                  | Path to XML for filter configuration                                       |
| LogRequestBody        | bool                           | `true`                                  | Logs the body of the request                                               |
| LogRequestCookies     | bool                           | `true`                                  | Logs the cookie values for the request                                     |
| LogRequestForm        | bool                           | `true`                                  | Logs the form values for the request                                       |
| LogSqlQueries         | bool                           | `true`                                  | Logs SQL queries using "SqlClientDiagnosticListener"                       |
| LogSqlQueryParameters | bool                           | `true`                                  | Logs parameter values for the SQL queries captured by `LogSqlQueries`      |
| Notifiers             | IErrorNotifier[]               | empty                                   | A collection of [`IErrorNotifier`](#using-notifiers) instances to send notifications on errors |
| OnError               | Func<HttpContext, Error, Task> | empty                                   | Callback that is executed before error is logged. Consumer can add or remove content to be logged in this callback. |
| ShowCelmahErrorPage   | bool                           | `false`                                 | Displays the Celmah UI when an error is captured                           |
| IgnoredStatusCodes    | int[]                          | `empty`                                 | HTTP status codes to skip when logging synthetic errors from response status codes (e.g. `[404]` to suppress 404 logs) |
| EnableIpGeoLookup     | bool                           | `false`                                 | Enable IP address geo-lookup (country flag) in error detail UI via `ip-api.com`. Requires outbound HTTP and permissive CSP. |
| SourcePaths           | string[]                       | empty                                   | Paths to source code to enrich stack traces                                |

**TIP**: :information_source: Celmah options work well with environment specific `appsettings` files. A `Configure` method exists on the builder to enable binding configuration to Celmah options.

```json
{
    "Celmah": {
        "LogRequestCookies": false,
        "ShowCelmahErrorPage": true
    }
}
```

```csharp
builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.Configure(builderContext.Configuration.GetSection("Celmah"));
});
```

### Ignoring specific HTTP status codes

By default, Celmah logs all responses with status codes 400–599 as errors. You can
suppress specific status codes (e.g. 404 Not Found) to reduce noise in production:

```csharp
builder.Host.UseCelmah((_, celmah) =>
{
    celmah.Configure(o => o.IgnoredStatusCodes = [404]);              // skip 404s
    // celmah.Configure(o => o.IgnoredStatusCodes = [404, 401, 403]); // skip multiple
});
```

Or via `appsettings.json`:

```json
{
    "Celmah": {
        "IgnoredStatusCodes": [404]
    }
}
```

## Restrict access to the Celmah UI

The `MapCelmah()` method registers the Celmah endpoints as regular endpoints in the application. As such, it can accept authorization policies just like any other endpoints in the application. Metadata can be applied to the returned endpoint collection.

```csharp
// allow all users to access UI
app.MapCelmah().AllowAnonymous();

// or require authenticated user
app.MapCelmah().RequireAuthorization();
```

> See .NET documentation for [Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction) for additional details.

## Error Persistence

The following persistence options are built into the core package:

- MemoryErrorLog – store errors in memory (default)
- XmlFileErrorLog – store errors in XML files

```csharp
using Celmah;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.PersistToFile("~/log"; /* OR "с:\errors" */);
});
```
<!-- #sql -->
- SqlErrorLog - store errors in MS SQL (add reference to Celmah.SqlServer and use `PersistToSql` method)
- MysqlErrorLog - store errors in MySQL (add reference to Celmah.MySql and use `PersistToMySql` method)
- PgsqlErrorLog - store errors in PostgreSQL (add reference to Celmah.Postgresql and use `PersistToPgsql` method)

```csharp
using Celmah;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.PersistToSql(options =>
    {
        options.ConnectionString = "connection_string";
        options.SqlServerDatabaseSchemaName = "Errors"; //Defaults to dbo if not set
        options.SqlServerDatabaseTableName = "CelmahError"; //Defaults to CELMAH_Error if not set
    });
});
```
<!-- #sql -->
<!-- #redis -->
- RedisErrorLog - store errors in Redis (add reference to Celmah.Redis and use `PersistToRedis` method)

```csharp
using Celmah;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.PersistToRedis(options =>
    {
        // Defaults
        options.RedisListKeyPrefix = "urn:celmah:error_list:";
        options.RedisKeyPrefix = "urn:celmah:error:";
        options.MaximumSize = 200; // (FIFO)
    });
});
```
<!-- #redis -->
You can implement your own error log persistence by implementing the abstract class `Celmah.ErrorLog` and register it using the builder method `celmah.PersistTo<YourErrorLog>()` (or one of the other `PersistTo` overloads).

## Using UseCelmahExceptionPage

Use `UseCelmahExceptionPage` (or the `ShowCelmahErrorPage` in Celmah options) to automatically display the Celmah UI diagnostics page when an uncaught exception occurs.

```csharp
builder.Host.UseCelmah((builderContext, celmah) =>
{
    if (builderContext.HostingEnvironment.IsDevelopment())
    {
        celmah.UseCelmahExceptionPage();
    }
});
```

> :warning: The Celmah diagnostics page can leak sensitive environmental details. Consider limiting the page to development environments or [placing security on the Celmah endpoints](#restrict-access-to-the-celmah-ui).

## Using Notifiers

You can create your own notifiers by implementing `IErrorNotifier` interface and add notifier to Celmah options. Each notifier must have a unique name.
(A notifier which generates emails is built into the library.)

```csharp
using Celmah;
using Celmah.Notifiers;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.Configure(options =>
    {
        options.Notifiers.Add(new ErrorMailNotifier("Email", emailOptions));
    });
});
```

## Using Filters

You can use Celmah XML filter configuration in a separate file or define them in code. Implement `IErrorFilter` to define custom filters in code. Filtered errors will be logged, but will not be sent.

```csharp
using Celmah;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.Configure(options =>
    {
        // Path to filters defined in XML
        options.FiltersConfig = "celmah.xml";

        // Add filters defined in code
        options.Filters.Add(new MyFilter());
    });
})
```

XML filter config example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<celmah>
  <errorFilter>
    <notifiers>
      <notifier name="Email"/>
    </notifiers>
    <test>
      <and>
        <greater binding="HttpStatusCode" value="399" type="Int32" />
        <lesser binding="HttpStatusCode" value="500" type="Int32" />
      </and>
    </test>
  </errorFilter>
</celmah>
```

See more details in [original documentation](https://elmah.github.io/a/error-filtering/examples/).

## Extensions

### Raise an Exception

To log a handled exception, use the `RaiseErrorAsync` extension method.

```csharp
using Celmah;

public async Task<IActionResult> Test()
{
    await HttpContext.RaiseErrorAsync(new InvalidOperationException("Test"));
}
```

### Logging method parameters

```csharp
using Celmah;

public void TestMethod(string p1, int p2)
{
    // Logging method parameters
    HttpContext.LogParamsToCelmah(this, p1, p2);
}
```
<!-- #serilog -->
### Serilog support

If you use [Serilog](https://serilog.net/) for logs and would like these logs to be included as context when Celmah captures errors, the `Celmah.Serilog` package can be used. This will add a `ILogEventSink` to DI which will be picked up by Serilog when the option to read configuration from services is used.

```csharp
using Celmah;

builder.Host.UseCelmah((builderContext, celmah) =>
{
    celmah.CaptureSerilogMessages();
});
```
<!-- #serilog -->

## NuGet Packages

| Package              | Description                              |
|----------------------|------------------------------------------|
| `Celmah`             | Core library with Vue UI                 |
| `Celmah.SqlServer`   | SQL Server error log persistence         |
| `Celmah.Postgresql`  | PostgreSQL error log persistence         |
| `Celmah.MySql`       | MySQL error log persistence              |
| `Celmah.Redis`       | Redis error log persistence              |
| `Celmah.Serilog`     | Serilog sink for Celmah                  |

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Bun](https://bun.sh/) (for building the Vue SPA)

### Build everything

```shell
./build-and-publish-local.sh
```

This script will:

1. **Build the Vue SPA** (`ui/` → `src/Celmah/wwwroot/`)
2. **Pack all NuGet packages** into `artifacts/package/release/`
3. **Publish** them to the local feed at `/mnt/c/git/nuget/Celmah`

### Manual step-by-step

```shell
# 1. Build the Vue frontend
cd ui
bun install
bun run build
cd ..

# 2. Pack individual projects
dotnet pack src/Celmah/Celmah.csproj -c Release
dotnet pack src/Celmah.SqlServer/Celmah.SqlServer.csproj -c Release
dotnet pack src/Celmah.Postgresql/Celmah.Postgresql.csproj -c Release

# 3. Publish to local feed
dotnet nuget push artifacts/package/release/Celmah.1.0.0.nupkg --source /mnt/c/git/nuget/Celmah
dotnet nuget push artifacts/package/release/Celmah.SqlServer.1.0.0.nupkg --source /mnt/c/git/nuget/Celmah
dotnet nuget push artifacts/package/release/Celmah.Postgresql.1.0.0.nupkg --source /mnt/c/git/nuget/Celmah
```

### Consuming local packages

Add a `nuget.config` to the consuming project:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="CelmahLocal" value="/mnt/c/git/nuget/Celmah" />
  </packageSources>
</configuration>
```

Then install as usual:

```shell
dotnet add package Celmah
dotnet add package Celmah.SqlServer
dotnet add package Celmah.Postgresql
```

## Differences from Upstream

This fork diverges from [jrsearles/Elmah.AspNetCore](https://github.com/jrsearles/Elmah.AspNetCore) in the following ways:

- **Renamed to Celmah** — all namespaces, types, and APIs use `Celmah` prefix
- **Targets .NET 10 only** (upstream targets .NET 6+)
- **Updated NuGet package dependencies** to latest versions
- **Removed multi-targeting** — single `net10.0` TFM
- **Local NuGet feed** support via `build-and-publish-local.sh`

## Migrating from Elmah.AspNetCore

A rough find-and-replace migration guide:

| Old (Elmah.AspNetCore) | New (Celmah) |
|---|---|
| `using Elmah.AspNetCore;` | `using Celmah;` |
| `UseElmah()` | `UseCelmah()` |
| `UseElmahMiddleware()` | `UseCelmahMiddleware()` |
| `MapElmah()` | `MapCelmah()` |
| `ElmahOptions` | `CelmahOptions` |
| `ElmahBuilder` | `CelmahBuilder` |
| `Elmah.AspNetCore.MsSql` | `Celmah.SqlServer` |
| `Elmah.AspNetCore.PostgreSql` | `Celmah.Postgresql` |
| `Elmah.AspNetCore.MySql` | `Celmah.MySql` |
| `Elmah.AspNetCore.StackExchange.Redis` | `Celmah.Redis` |
| `Serilog.Sinks.Elmah.AspNetCore` | `Celmah.Serilog` |
