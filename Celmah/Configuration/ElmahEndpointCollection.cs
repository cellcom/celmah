#if NET6_0
using System;
using Microsoft.AspNetCore.Builder;

namespace Celmah;

internal class CelmahEndpointCollection : IEndpointConventionBuilder
{
    private readonly IEndpointConventionBuilder[] _routes;

    public CelmahEndpointCollection(IEndpointConventionBuilder[] routes)
    {
        _routes = routes;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var route in _routes)
        {
            route.Add(convention);
        }
    }
}
#endif
