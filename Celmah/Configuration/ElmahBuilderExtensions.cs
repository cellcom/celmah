using System;
using Celmah.Logger;
using Celmah.Memory;
using Celmah.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Celmah;

public static class CelmahBuilderExtensions
{
    public static void UseCelmahExceptionPage(this ICelmahBuilder builder)
    {
        builder.Configure(o => o.ShowCelmahErrorPage = true);
    }

    public static void Configure(this ICelmahBuilder builder, Action<CelmahOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
    }

    public static void Configure(this ICelmahBuilder builder, IConfiguration configuration)
    {
        builder.Services.Configure<CelmahOptions>(configuration);
    }

    public static void PersistToMemory(this ICelmahBuilder builder)
    {
        builder.PersistToMemory(o => { });
    }

    public static void PersistToMemory(this ICelmahBuilder builder, Action<MemoryErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new MemoryErrorLog(provider.GetRequiredService<IOptions<MemoryErrorLogOptions>>()));
    }

    public static void PersistToFile(this ICelmahBuilder builder, string logPath)
    {
        builder.PersistToFile(o => o.LogPath = logPath); 
    }

    public static void PersistToFile(this ICelmahBuilder builder, Action<XmlFileErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo<XmlFileErrorLog>();
    }

    public static void SetLogLevel(this ICelmahBuilder builder, LogLevel level)
    {
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter<CelmahLoggerProvider>(l => l >= level); 
        });
    }
}