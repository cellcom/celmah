using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Celmah.Serilog;

namespace Celmah;

public static class CelmahSerilogBuilderExtensions
{
    /// <summary>
    /// Captures log messages sent through Serilog to be presented in Elmah UI along with error context.
    /// </summary>
    /// <param name="builder"></param>
    public static void CaptureSerilogMessages(this ICelmahBuilder builder)
    {
        builder.Services.AddSingleton<ILogEventSink, CelmahSink>();
    }
}