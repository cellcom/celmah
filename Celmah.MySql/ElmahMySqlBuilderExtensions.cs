using System;
using Celmah.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Celmah;

public static class CelmahMySqlBuilderExtensions
{
    public static void PersistToMySql(this ICelmahBuilder builder, Action<MySqlErrorLogOptions> configureOptions)
    {
        builder.Services.Configure(configureOptions);
        builder.PersistTo(provider => new MySqlErrorLog(provider.GetRequiredService<IOptions<MySqlErrorLogOptions>>()));
    }
}
