using Avalonia;
using Avalonia.Controls;

namespace Client.Avalonia.Extensions;

public static class ApplicationExtensions
{
    public static TResource? GetResourceOrDefault<TResource>(this Application @this, string key)
    {
        return @this.TryGetResource(key, out var resource) ? (TResource?)resource : default;
    }
}