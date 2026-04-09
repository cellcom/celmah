using System;

namespace Celmah;

public class CelmahLogSqlEntry
{
    public DateTime TimeStamp { get; set; }
    public long TimerStart { get; set; }
    public string? SqlText { get; set; }
    public string? CommandType { get; set; }
    public double DurationMs { get; set; }
}