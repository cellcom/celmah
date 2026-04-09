using System;

namespace Celmah;

public interface ICelmahFeature
{
    public Guid Id { get; }

    public string Location { get; }
}
