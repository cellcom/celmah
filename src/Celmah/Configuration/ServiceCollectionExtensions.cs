using System;
using Celmah.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Celmah;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCelmahCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<ICelmahExceptionLogger, CelmahExceptionLogger>();
        services.AddSingleton<ILoggerProvider, CelmahLoggerProvider>();
        services.AddSingleton<IErrorFactory, ErrorFactory>();
        services.AddSingleton<CelmahSqlDiagnosticObserver>();
        services.AddSingleton(new CelmahEnvironment());
        
#if USE_GLOBAL_ERROR_HANDLING
        services.AddExceptionHandler<CelmahExceptionHandler>();
#endif

        return services;
    }
}
