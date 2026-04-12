using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using Celmah.Serilog;

namespace Celmah;

public static class CelmahSerilogBuilderExtensions
{
    /// <summary>
    /// Captures log messages sent through Serilog to be presented in Elmah UI along with error context.
    /// Requires <c>ReadFrom.Services(services)</c> in the Serilog configuration to resolve the sink from DI.
    /// </summary>
    /// <param name="builder"></param>
    public static void CaptureSerilogMessages(this ICelmahBuilder builder)
    {
        builder.Services.AddSingleton<ILogEventSink, CelmahSink>();
    }

    /// <summary>
    /// Writes Serilog log events to the Celmah error log for display in the Celmah UI.
    /// Use this in your <c>UseSerilog</c> callback to explicitly add the Celmah sink to the Serilog pipeline.
    /// This is the recommended approach as it does not rely on <c>ReadFrom.Services</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Host.UseCelmah((_, celmah) => {
    ///     celmah.PersistToMemory();
    /// });
    /// 
    /// builder.Host.UseSerilog((context, services, configuration) => configuration
    ///     .WriteTo.CelmahSink(services)
    ///     .WriteTo.Console());
    /// </code>
    /// </example>
    public static LoggerConfiguration CelmahSink(
        this LoggerSinkConfiguration sinkConfiguration,
        IServiceProvider serviceProvider,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch? levelSwitch = null)
    {
        var httpContextAccessor = serviceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        if (httpContextAccessor is null)
            throw new InvalidOperationException(
                "IHttpContextAccessor is not registered. Ensure AddCelmahCoreServices() or AddHttpContextAccessor() has been called.");

        return sinkConfiguration.Sink(new CelmahSink(httpContextAccessor), restrictedToMinimumLevel, levelSwitch);
    }
}