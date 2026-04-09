using Celmah;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Celmah.Serilog;

/// <summary>
/// CelmahCore will wire up an <see cref="ILoggerProvider"/> to use with MEL, however Serilog
/// ignores these providers, instead using their <see cref="ILogEventSink"/> instances. This
/// creates a bridge between the two.
/// </summary>
public sealed class CelmahSink : ILogEventSink
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CelmahSink(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Emit(LogEvent logEvent)
    {
        var elmahFeature = _httpContextAccessor.HttpContext?.Features.Get<ICelmahLogFeature>();
        if (elmahFeature is null)
        {
            return;
        }

        LogLevel level = CelmahSink.GetLogLevelFromSerilog(logEvent.Level);

        var entry = new CelmahSerilogMessage
        {
            TimeStamp = DateTime.UtcNow,
            Exception = logEvent.Exception?.ToString(),
            Level = level,
            Template = logEvent.MessageTemplate,
            Properties = logEvent.Properties
        };

        elmahFeature.AddMessage(entry);
    }

    private static LogLevel GetLogLevelFromSerilog(LogEventLevel level) =>
      level switch
      {
          LogEventLevel.Verbose => LogLevel.Trace,
          LogEventLevel.Debug => LogLevel.Debug,
          LogEventLevel.Information => LogLevel.Information,
          LogEventLevel.Warning => LogLevel.Warning,
          LogEventLevel.Error => LogLevel.Error,
          LogEventLevel.Fatal => LogLevel.Critical,
          _ => LogLevel.None
      };
}
