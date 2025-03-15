using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Client.Avalonia.Extensions;

public static class ReactiveExtensions
{
    static internal IObservable<T> WhereNotNull<T>(this IObservable<T?> observable) =>
        observable.Where(x => x is not null).Select(x => x!);

    /// <summary>
    /// Делает IObservable общим для всех подписчиков. Т.е. цепочка преобразований не будет выполняться при каждой новой подписке
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="this"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    public static IObservable<TData> Share<TData>(this IObservable<TData> @this, int bufferSize = 1) =>
        @this.Replay(bufferSize).RefCount();

    public static async Task<TData> Value<TData>(this IObservable<TData> obs)
    {
        return await obs.Take(1).LastAsync();
    }
}