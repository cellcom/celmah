#if USE_GLOBAL_ERROR_HANDLING
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Celmah;

internal class CelmahExceptionHandler : IExceptionHandler
{
    private readonly ICelmahExceptionLogger _elmahLogger;
    private readonly IOptions<CelmahOptions> _elmahOptions;

    public CelmahExceptionHandler(ICelmahExceptionLogger elmahLogger, IOptions<CelmahOptions> elmahOptions)
    {
        _elmahLogger = elmahLogger;
        _elmahOptions = elmahOptions;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var entry = await _elmahLogger.LogExceptionAsync(httpContext, exception);

        string? location = null;
        if (entry is not null)
        {
            location = $"{httpContext.GetCelmahRelativeRoot()}/detail/{entry.Id}";
            httpContext.Features.Set<ICelmahFeature>(new CelmahFeature(entry.Id, location));
        }

        if (!string.IsNullOrEmpty(location) && _elmahOptions.Value.ShowCelmahErrorPage)
        {
            httpContext.Response.StatusCode = ErrorFactory.GetStatusCodeFromExceptionOr500(exception);
            if (httpContext.RequestAcceptsHtml())
            {
                httpContext.Response.Redirect(location);
            }

            return true;
        }

        return false;
    }
}
#endif