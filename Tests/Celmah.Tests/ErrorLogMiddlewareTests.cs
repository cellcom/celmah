using System;
using System.Threading.Tasks;
using Celmah.Memory;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Celmah.Tests;

public class ErrorLogMiddlewareTests
{
    private readonly RequestDelegate _requestDelegate;
    private readonly ErrorLog _errorLog = new MemoryErrorLog();
    private readonly IOptions<CelmahOptions> _options;
    private readonly ICelmahExceptionLogger _elmahLogger;

    public ErrorLogMiddlewareTests()
    {
        _requestDelegate = httpContext => Task.CompletedTask;
        _options = Options.Create(new CelmahOptions());
        _elmahLogger = new CelmahExceptionLogger(_errorLog, new ErrorFactory(_options), _options, NullLogger<CelmahExceptionLogger>.Instance);
    }

    [Fact]
    public void RiseErrorOkWhenMiddlewareInitialized()
    {
        var _ = new ErrorLogMiddleware(_requestDelegate, _elmahLogger, _options);
        var httpContext = new DefaultHttpContext();

        var services = new ServiceCollection();
        services.AddSingleton(_elmahLogger);
        httpContext.RequestServices = services.BuildServiceProvider();

        Func<Task> act = async () => await CelmahExtensions.RaiseErrorAsync(httpContext, new Exception());
        act.Should().NotThrowAsync();
    }
}