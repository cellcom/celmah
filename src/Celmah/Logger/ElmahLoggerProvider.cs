using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Celmah.Logger;

internal sealed class CelmahLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IExternalScopeProvider? _scopeProvider;

    public CelmahLoggerProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string name)
    {
        return new CelmahLogger(name, null, _scopeProvider, _httpContextAccessor);
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}