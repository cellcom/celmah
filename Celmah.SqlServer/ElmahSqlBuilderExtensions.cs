using Celmah.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Celmah;

public static class CelmahSqlBuilderExtensions
{
    public static void PersistToSql(this ICelmahBuilder builder, Action<SqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new SqlErrorLog(provider.GetRequiredService<IOptions<SqlErrorLogOptions>>()));
    }
}
