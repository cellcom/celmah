using System;
using Microsoft.Extensions.Logging;

namespace Celmah.Logger;

internal sealed class CelmahLoggerMessage<TState> : ICelmahLogMessage
{
    public DateTime TimeStamp { get; init; }
    public string? Scope { get; init; }
    public Exception? Exception { get; init; }
    string? ICelmahLogMessage.Exception => this.Exception?.ToString();
    public LogLevel? Level { get; init; }
    public TState State { get; init; } = default!;
    public Func<TState, Exception?, string> Formatter { get; init; } = default!;
    public string Render() => this.Formatter(this.State, this.Exception);
}
