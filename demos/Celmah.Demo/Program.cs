using Celmah;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseCelmah((_, celmah) =>
{
    celmah.UseCelmahExceptionPage();
    celmah.PersistToMemory();
    celmah.Configure(o => o.IgnoredStatusCodes = [404]);
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseCelmahMiddleware();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseRouting();

app.MapCelmah();

app.MapGet("/", () => Results.Text("""
    <!DOCTYPE html>
    <html><body>
    <h1>Celmah Demo</h1>
    <ul>
        <li><a href="/celmah">View Errors (Celmah UI)</a></li>
        <li><a href="/error">Throw Exception (handled by middleware)</a></li>
        <li><a href="/raise">Raise Error (explicit log via RaiseErrorAsync)</a></li>
        <li><a href="/log">Log via ILogger (shows message log in error detail)</a></li>
        <li><a href="/log-sql">Log with SQL diagnostic messages</a></li>
        <li><a href="/params?phone=1234&id=5678">Log with method parameters</a></li>
    </ul>
    </body></html>
    """, "text/html"));

// Thrown exception — caught by Celmah middleware, no VS debug break
app.MapGet("/error", (HttpContext ctx) =>
{
    try
    {
        throw new InvalidOperationException("Test exception from Celmah Demo");
    }
    catch (Exception ex)
    {
        return Task.FromResult(ctx.RaiseErrorAsync(ex));
    }
});

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

app.Run();
