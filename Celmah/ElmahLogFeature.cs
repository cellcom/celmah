using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Celmah;

internal class CelmahLogFeature : ICelmahLogFeature
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 0
    };

    private readonly ConcurrentDictionary<Guid, CelmahLogSqlEntry> _map = new();
    private readonly ConcurrentBag<ICelmahLogMessage> _logs = new();
    private readonly ConcurrentBag<CelmahLogParamEntry> _params = new();

    public IReadOnlyCollection<ICelmahLogMessage> Log => _logs.ToList();
    public IReadOnlyCollection<CelmahLogParamEntry> Params => _params.ToList();
    public IReadOnlyCollection<CelmahLogSqlEntry> LogSql => _map.Values.OrderBy(i => i.TimeStamp).ToList();

    public void AddMessage(ICelmahLogMessage entry)
    {
        _logs.Add(entry);
    }

    public void AddSql(Guid id, CelmahLogSqlEntry entry)
    {
        _map.TryAdd(id, entry);
    }

    public void SetSqlDuration(Guid id)
    {
        if (_map.TryGetValue(id, out CelmahLogSqlEntry? data))
        {
            data.DurationMs = StopwatchExtensions.GetElapsedTime(data.TimerStart).TotalMilliseconds;
        }
    }

    public void LogParameters((string name, object? value)[] list, string typeName, string memberName,
        string file, int line)
    {
        var paramList = list.Where(x => x != default).Select(x => new KeyValuePair<string, string>(x.name, ValueToString(x.value))).ToArray();
        _params.Add(new CelmahLogParamEntry(DateTime.UtcNow, paramList, typeName, memberName, file, line));
    }

    private static string ValueToString(object? paramValue)
    {
        if (paramValue is null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(paramValue, SerializerOptions);
        }
        catch
        {
            return paramValue.ToString()!;
        }
    }
}
