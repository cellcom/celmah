using System.Diagnostics.CodeAnalysis;
using Celmah;
using Celmah.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Celmah;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapCelmah(this IEndpointRouteBuilder endpoints) => endpoints.MapCelmah("/celmah");

    public static IEndpointConventionBuilder MapCelmah(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string prefix)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<CelmahEnvironment>();
        options.Path = prefix;

#if NET7_0_OR_GREATER
        var group = endpoints.MapGroup(prefix);
        group.MapRoot();
        group.MapApiError();
        group.MapApiErrors();
        group.MapApiNewErrors();
        group.MapRss();
        group.MapDigestRss();
        group.MapMsdn();
        group.MapMsdnStatus();
        group.MapJson();
        group.MapXml();
        group.MapDownload();
        group.MapTest();
        group.MapResources();
        return group
            .WithDisplayName("Celmah");
#else
        var routes = new[]
        {
            endpoints.MapRoot(prefix),
            endpoints.MapApiError(prefix),
            endpoints.MapApiErrors(prefix),
            endpoints.MapApiNewErrors(prefix),
            endpoints.MapRss(prefix),
            endpoints.MapDigestRss(prefix),
            endpoints.MapMsdn(prefix),
            endpoints.MapMsdnStatus(prefix),
            endpoints.MapXml(prefix),
            endpoints.MapJson(prefix),
            endpoints.MapDownload(prefix),
            endpoints.MapTest(prefix),
            endpoints.MapResources(prefix)
        };

        return new CelmahEndpointCollection(routes)
            .WithDisplayName("Celmah");
#endif
    }
}
