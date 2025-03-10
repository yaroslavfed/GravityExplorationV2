namespace Client.Core.Holders;

public interface IHolderService<TData>
{
    public IObservable<IReadOnlyList<TData>> DataList { get; }

    public Task Add(TData data);

    public Task Remove(Guid id);

    public Task Update(TData data);
}