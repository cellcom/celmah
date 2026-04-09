using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Celmah;

internal sealed class ErrorLogMiddleware
{
    private readonly ICelmahExceptionLogger _elmahLogger;
    private readonly IOptions<CelmahOptions> _options;
    private readonly RequestDelegate _next;
    
    public ErrorLogMiddleware(
        RequestDelegate next,
        ICelmahExceptionLogger elmahLogger,
        IOptions<CelmahOptions> elmahOptions)
    {
        _next = next;
        _elmahLogger = elmahLogger;
        _options = elmahOptions;
    }

    public Task InvokeAsync(HttpContext context)
    {
#if USE_GLOBAL_ERROR_HANDLING
        // Dotnet 8+ we will use built-in exception middleware, but need to handle cases with error status codes
        // and attach feature for consumers to access.
        return this.ExecuteMiddlewareAsync(context);
#else
        ExceptionDispatchInfo exceptionInfo;
        try
        {
            var task = this.ExecuteMiddlewareAsync(context);
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(this, context, task);
            }

            return task;
        }
        catch (Exception exception)
        {
            exceptionInfo = ExceptionDispatchInfo.Capture(exception);
        }

        return this.HandleExceptionAsync(context, exceptionInfo);

        async Task Awaited(ErrorLogMiddleware middleware, HttpContext context, Task task)
        {
            ExceptionDispatchInfo? exceptionInfo = null;
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(exception);
            }

            if (exceptionInfo is not null)
            {
                await middleware.HandleExceptionAsync(context, exceptionInfo);
            }
        }
#endif
    }

    private async Task HandleExceptionAsync(HttpContext context, ExceptionDispatchInfo exceptionInfo)
    {
        var entry = await _elmahLogger.LogExceptionAsync(context, exceptionInfo.SourceException);

        string? location = null;
        if (entry is not null)
        {
            location = $"{context.GetCelmahRelativeRoot()}/detail/{entry.Id}";
            context.Features.Set<ICelmahFeature>(new CelmahFeature(entry.Id, location));
        }

        //To next middleware
        if (entry is null || !_options.Value.ShowCelmahErrorPage)
        {
            exceptionInfo.Throw();
            return;
        }

        //Show Debug page
        context.Response.StatusCode = ErrorFactory.GetStatusCodeFromExceptionOr500(exceptionInfo.SourceException);
        if (context.RequestAcceptsHtml())
        {
            context.Response.Redirect(location!);
        }
    }

    private async Task ExecuteMiddlewareAsync(HttpContext context)
    {
        context.Features.Set<ICelmahLogFeature>(new CelmahLogFeature());

        await _next(context);

        if (context.Response.HasStarted
            || context.Response.StatusCode < 400
            || context.Response.StatusCode >= 600
            || context.Response.ContentLength.HasValue
            || !string.IsNullOrEmpty(context.Response.ContentType)
            || context.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        var exception = new BadHttpRequestException("An error status was returned when processing the request", context.Response.StatusCode);
        await _elmahLogger.LogExceptionAsync(context, exception);
    }
}