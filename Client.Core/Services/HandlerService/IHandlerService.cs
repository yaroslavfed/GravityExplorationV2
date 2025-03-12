namespace Client.Core.Services.HandlerService;

public interface IHandlerService<TData>
{
    public IObservable<IReadOnlyList<TData>> UpdatedData { get; }

    public Task AddAsync(TData data);

    public Task UpdateAsync(TData data);

    public Task RemoveAsync(Guid id);
}