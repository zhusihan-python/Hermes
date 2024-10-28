using System.Collections.Generic;
using System.Linq;
using System;

namespace Hermes.Common.Extensions;

public static class NullableExtensions
{
    public static IEnumerable<T?> GetValues<T>() where T : struct, Enum
    {
        return EnumExtensions
            .GetValues<T>()
            .Select(x => (T?)x)
            .Prepend(null)
            .ToList();
    }
}