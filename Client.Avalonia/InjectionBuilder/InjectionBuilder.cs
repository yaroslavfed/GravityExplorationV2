using System;
using System.Collections.Generic;
using System.Linq;
using Client.Avalonia.Extensions;
using Splat;

namespace Client.Avalonia.InjectionBuilder;

public class InjectionBuilder<TService>
{
    private readonly IReadonlyDependencyResolver         _dependencyResolver;
    private readonly IList<(Type Type, object Instance)> _autocompleteMap = new List<(Type Type, object Instance)>();

    internal InjectionBuilder(IReadonlyDependencyResolver dependencyResolver)
    {
        _dependencyResolver = dependencyResolver;
    }

    /// <summary>
    /// Добавляет в список пользовательскую зависимость по указанному типу. Кастомные зависимости должны указываться в конструкторе перед внедряемыми автоматически.
    /// </summary>
    /// <typeparam name="TDependency"></typeparam>
    /// <param name="instance"></param>
    /// <returns></returns>
    public InjectionBuilder<TService> WithAutocomplete<TDependency>(TDependency instance)
    {
        _autocompleteMap.Add((typeof(TDependency), instance)!);

        return this;
    }

    /// <summary>
    /// Конструирует объект на основе зарегистрированных с помощью WithAutocomplete зависимостей и зависимостей, зарегистрированных в контейнере.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public TService BuildService()
    {
        var serviceType = typeof(TService);
        var dependenciesQuantity = _autocompleteMap.Count;

        var constructor = serviceType
                          .GetConstructors()
                          .FirstOrDefault(c => c.GetParameters().Length >= dependenciesQuantity);

        if (constructor is null) throw new ArgumentException("No matching constructor");

        var inputDependenciesTypeList = _autocompleteMap.Select(dependencyEntry => dependencyEntry.Type).ToHashSet();

        var dependenciesToResolve = constructor
                                    .GetParameters()
                                    .Where(parameter => !inputDependenciesTypeList.Contains(parameter.ParameterType));

        var resolvedDependencyList = dependenciesToResolve.Select(
            dependency => _dependencyResolver.GetRequiredService(dependency.ParameterType)
        );

        var autocompleteDependencyList = _autocompleteMap.Select(dependencyEntry => dependencyEntry.Instance);
        var fullDependenciesList = autocompleteDependencyList.Concat(resolvedDependencyList).ToArray();

        return (TService)constructor.Invoke(fullDependenciesList);
    }
}