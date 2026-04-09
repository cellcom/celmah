using Celmah;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Celmah.Serilog;

internal sealed class CelmahSerilogMessage : ICelmahLogMessage
{
    public MessageTemplate Template { get; init; } = default!;

    public IReadOnlyDictionary<string, LogEventPropertyValue> Properties { get; init; } = default!;

    public DateTime TimeStamp { get; init; }

    public string? Exception { get; init; }

    public string? Scope { get; init; }

    public LogLevel? Level { get; init; }

    public string? Render() => this.Template.Render(this.Properties, null);
}
