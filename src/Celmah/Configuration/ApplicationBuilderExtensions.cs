using System;
using System.Diagnostics;
using Celmah;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Celmah;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCelmahMiddleware(this IApplicationBuilder app)
    {
        // This is not related to middleware but need to execute this during startup - rather
        // than require another method, we're just hitching on to this method.
        var observer = app.ApplicationServices.GetService<CelmahSqlDiagnosticObserver>();
        if (observer is not null)
        {
            DiagnosticListener.AllListeners.Subscribe(observer);
        }

        app.UseMiddleware<ErrorLogMiddleware>();
        return app;
    }

    public static IHostBuilder UseCelmah(this IHostBuilder host)
    {
        return host.UseCelmah((Action<HostBuilderContext, CelmahBuilder>)null!);
    }

    public static IHostBuilder UseCelmah(this IHostBuilder host, Action<CelmahBuilder> configureCelmah)
    {
        return host.UseCelmah((_, celmah) => configureCelmah?.Invoke(celmah));
    }

    public static IHostBuilder UseCelmah(this IHostBuilder host, Action<HostBuilderContext, CelmahBuilder> configureCelmah)
    {
        return host.ConfigureServices((builderContext, services) => ConfigureCelmah(builderContext, services, configureCelmah));
    }

    public static IWebHostBuilder UseCelmah(this IWebHostBuilder host)
    {
        return host.UseCelmah((Action<WebHostBuilderContext, CelmahBuilder>)null!);
    }

    public static IWebHostBuilder UseCelmah(this IWebHostBuilder host, Action<CelmahBuilder> configureCelmah)
    {
        return host.UseCelmah((_, celmah) => configureCelmah?.Invoke(celmah));
    }

    public static IWebHostBuilder UseCelmah(this IWebHostBuilder host, Action<WebHostBuilderContext, CelmahBuilder> configureCelmah)
    {
        return host.ConfigureServices((builderContext, services) => ConfigureCelmah(builderContext, services, configureCelmah));
    }

    private static void ConfigureCelmah<TContext>(TContext context, IServiceCollection services, Action<TContext, CelmahBuilder> configureCelmah)
    {
        services.AddCelmahCoreServices();

        var celmah = new CelmahBuilder(services);

        // Set as default because it is required - consumer can replace in configure delegate
        celmah.PersistToMemory();

        configureCelmah?.Invoke(context, celmah);
    }
}
