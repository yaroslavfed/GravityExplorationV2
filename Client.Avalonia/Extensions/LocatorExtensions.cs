using System;
using Client.Avalonia.InjectionBuilder;
using Splat;

namespace Client.Avalonia.Extensions;

public static class LocatorExtensions
{
    public static TService GetRequiredService<TService>(this IReadonlyDependencyResolver @this)
    {
        var service = @this.GetService<TService>();

        if (service is null)
            throw new NullReferenceException($"Service {typeof(TService).FullName} was not registered");

        return service;
    }

    public static object GetRequiredService(this IReadonlyDependencyResolver @this, Type serviceType)
    {
        var service = @this.GetService(serviceType);

        if (service is null)
            throw new NullReferenceException($"Service {serviceType.FullName} was not registered");

        return service;
    }

    public static InjectionBuilder<TService> WithBuilder<TService>(this IReadonlyDependencyResolver @this)
    {
        return new InjectionBuilder<TService>(@this);
    }
}