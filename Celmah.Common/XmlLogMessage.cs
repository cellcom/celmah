using System;
using Microsoft.Extensions.Logging;

namespace Celmah;

public sealed class XmlLogMessage : ICelmahLogMessage
{
    public DateTime TimeStamp { get; set; }

    public string? Exception { get; set; }

    public string? Scope { get; set; }

    public LogLevel? Level { get; set; }

    public string? Message { get; set; }

    public string? Render() => this.Message;
}
