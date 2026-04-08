using System;
using Celmah.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Celmah;

public static class CelmahPgsqlBuilderExtensions
{
    public static void PersistToPgsql(this ICelmahBuilder builder, Action<PgsqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new PgsqlErrorLog(provider.GetRequiredService<IOptions<PgsqlErrorLogOptions>>()));
    }
}
