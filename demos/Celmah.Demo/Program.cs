using System.Diagnostics;
using System.Text;
using Celmah;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseCelmah((_, celmah) =>
{
    celmah.PersistToMemory();
});

// Configure Serilog with explicit CelmahSink registration.
// This is the recommended approach: call .WriteTo.CelmahSink(services) directly.
// Alternatively, use celmah.CaptureSerilogMessages() + .ReadFrom.Services(services)
// but the explicit WriteTo.CelmahSink() approach is more reliable.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .WriteTo.CelmahSink(services)
    .WriteTo.Console());

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseCelmahMiddleware();

app.UseStaticFiles();
app.UseRouting();

app.MapCelmah();

app.MapGet("/", () => Results.Text("""
<!DOCTYPE html>
<html>
<head>
    <title>Celmah Demo</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@picocss/pico@2.1.1/css/pico.min.css" />
</head>
<body>
    <h1>Celmah Demo</h1>
    <ul>
        <li><a href="/celmah">View Errors (Celmah UI)</a></li>
        <li><a href="/error">Throw Exception (handled by middleware)</a></li>
        <li><a href="/raise">Raise Error (explicit log via RaiseErrorAsync)</a></li>
        <li><a href="/big-error">Long message &amp; deep stack trace (UI stress test)</a></li>
        <li><a href="/log">Log via ILogger (shows message log in error detail)</a></li>
        <li><a href="/log-sql">Log with SQL diagnostic messages</a></li>
        <li><a href="/params?phone=1234&id=5678">Log with method parameters</a></li>
        <li><a href="/serilog">Log via Serilog (test CelmahSink integration)</a></li>
    </ul>
</body>
</html>
""", "text/html"));

// Thrown exception — caught by Celmah middleware, no VS debug break
app.MapGet("/error", [DebuggerHidden] (HttpContext ctx) => throw new InvalidOperationException("Test exception from Celmah Demo"));

// Very long message + deep stack trace, for exercising the error-detail UI
// (header message truncation, the dedicated Message tab, scrollable panes)
app.MapGet("/big-error", (HttpContext ctx) => ctx.RaiseErrorAsync(BuildDeepException(BuildLongMessage())));

// Explicitly raised error
app.MapGet("/raise", (HttpContext ctx) => ctx.RaiseErrorAsync(new Exception("Raised via RaiseErrorAsync")));

// ILogger integration — messages logged during the request appear in the error's MessageLog
app.MapGet("/log", (HttpContext ctx) =>
{
    logger.LogInformation("Processing request at {Path}", ctx.Request.Path);
    logger.LogWarning("Something suspicious happened");
    logger.LogError("Simulated error during processing");
    return ctx.RaiseErrorAsync(new Exception("Error with captured log messages"));
});

// SQL diagnostic messages captured
app.MapGet("/log-sql", (HttpContext ctx) =>
{
    logger.LogInformation("Querying database...");
    return ctx.RaiseErrorAsync(new Exception("Error with SQL context"));
});

// Method parameter logging
app.MapGet("/params", (HttpContext ctx, string? phone, string? id) =>
{
    ctx.LogParamsToElmah(new { phone, id });
    return ctx.RaiseErrorAsync(new Exception("Error with logged parameters"));
});

// Serilog sink test - log messages through MEL ILogger (which goes through Serilog pipeline)
// and verify they appear in the Celmah error detail
app.MapGet("/serilog", (HttpContext ctx) =>
{
    logger.LogInformation("Serilog test: Processing request at {Path}", ctx.Request.Path);
    logger.LogWarning("Serilog test: Something suspicious happened");
    logger.LogError("Serilog test: Simulated error during processing");
    return ctx.RaiseErrorAsync(new Exception("Error with Serilog-captured log messages"));
});

app.Run();

static string BuildLongMessage()
{
    var sb = new StringBuilder();
    sb.Append("An unexpected error occurred while processing the request.");
    sb.Append(" This deliberately verbose message exercises the Celmah error-detail UI");
    sb.Append(" (header message truncation, the dedicated Message tab, and long-content scrolling).");
    // ten long lines, each describing a different aspect of the failure
    for (int i = 1; i <= 10; i++)
    {
        sb.AppendLine();
        sb.Append($"Failure detail line {i}: ");
        for (int j = 1; j <= 8; j++)
        {
            sb.Append($"the subsystem reported an invalid configuration value at checkpoint {i}.{j}, ");
            sb.Append("which caused the pipeline to abort and roll back the in-flight transaction; ");
        }
    }
    return sb.ToString();
}

// A chain of distinct methods (no recursion). The runtime folds recursive/cyclic
// frames in the captured stack trace, so each step is its own method; this yields
// a genuinely long stack trace (with real source locations) for exercising the
// error-detail UI's scrollable panes.
static Exception BuildDeepException(string message)
{
    try { Step01(message); return null!; }
    catch (Exception ex) { return ex; }
}

static void Step01(string m) => Step02(m);
static void Step02(string m) => Step03(m);
static void Step03(string m) => Step04(m);
static void Step04(string m) => Step05(m);
static void Step05(string m) => Step06(m);
static void Step06(string m) => Step07(m);
static void Step07(string m) => Step08(m);
static void Step08(string m) => Step09(m);
static void Step09(string m) => Step10(m);
static void Step10(string m) => Step11(m);
static void Step11(string m) => Step12(m);
static void Step12(string m) => Step13(m);
static void Step13(string m) => Step14(m);
static void Step14(string m) => Step15(m);
static void Step15(string m) => Step16(m);
static void Step16(string m) => Step17(m);
static void Step17(string m) => Step18(m);
static void Step18(string m) => Step19(m);
static void Step19(string m) => Step20(m);
static void Step20(string m) => Step21(m);
static void Step21(string m) => Step22(m);
static void Step22(string m) => Step23(m);
static void Step23(string m) => Step24(m);
static void Step24(string m) => Step25(m);
static void Step25(string m) => Step26(m);
static void Step26(string m) => Step27(m);
static void Step27(string m) => Step28(m);
static void Step28(string m) => Step29(m);
static void Step29(string m) => Step30(m);
static void Step30(string m) => throw new InvalidOperationException(m);
