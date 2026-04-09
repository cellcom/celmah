using System;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Celmah.Handlers;

internal static partial class Endpoints
{
    private static readonly Assembly ThisAssembly = typeof(Endpoints).Assembly;
    private static readonly EmbeddedFileProvider StaticFiles = new(ThisAssembly, $"{ThisAssembly.GetName().Name}.wwwroot");

    private static async Task<IResult> ReturnIndex([FromServices] ILoggerFactory loggerFactory, HttpContext context)
    {
        var indexFile = StaticFiles.GetFileInfo("index.html");
        if (indexFile is not { Exists: true })
        {
            var logger = loggerFactory.CreateLogger("Celmah");
            logger.LogError("{page} is not found for Celmah - has static content been generated? See 'Running Source Locally' in README.md for more details.", "index.html");
            return Results.NotFound();
        }

        using var stream = indexFile.CreateReadStream();
        using var reader = new StreamReader(stream);

        var celmahRoot = context.GetCelmahRelativeRoot();
        var html = await reader.ReadToEndAsync();

        // Inject meta tag for JS to read the runtime root path
        var celmahOptions = context.RequestServices.GetRequiredService<IOptions<CelmahOptions>>();
        html = html.Replace("<head>", $"<head><meta name=\"celmah-root\" content=\"{celmahRoot}\"><meta name=\"celmah-ip-geo\" content=\"{(celmahOptions.Value.EnableIpGeoLookup ? "true" : "false")}\">");

        // Rewrite relative asset paths to absolute so they work from any client-side route
        // e.g. src="./assets/index.js" → src="/celmah/assets/index.js"
        html = html.Replace("src=\"./", $"src=\"{celmahRoot}/");
        html = html.Replace("href=\"./", $"href=\"{celmahRoot}/");

        return Results.Content(html, MediaTypeNames.Text.Html);
    }

    public static IEndpointConventionBuilder MapRoot(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var handler = RequestDelegateFactory.Create(ReturnIndex);

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.MapGet(prefix, pipeline.Build());
    }

    public static IEndpointConventionBuilder MapResources(this IEndpointRouteBuilder builder, string prefix = "")
    {
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        
        var handler = RequestDelegateFactory.Create(async ([FromRoute] string path, [FromServices] ILoggerFactory loggerFactory, HttpContext context) =>
        {
            if (!path.Contains('.', StringComparison.Ordinal))
            {
                return await ReturnIndex(loggerFactory, context);
            }

            var fileInfo = StaticFiles.GetFileInfo(path);
            if (fileInfo is not { Exists: true })
            {
                return Results.NotFound();
            }

            contentTypeProvider.TryGetContentType(path, out string? contentType);
            return Results.Stream(fileInfo.CreateReadStream(), contentType);
        });

        var pipeline = builder.CreateApplicationBuilder();
        pipeline.Run(handler.RequestDelegate);
        return builder.Map($"{prefix}/{{*path}}", pipeline.Build());
    }
}
