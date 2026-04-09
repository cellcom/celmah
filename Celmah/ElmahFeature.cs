using System;

namespace Celmah;

internal record CelmahFeature(Guid Id, string Location) : ICelmahFeature;
