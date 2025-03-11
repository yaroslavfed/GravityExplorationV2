using System.Reactive.Subjects;
using Client.Core.Data.Entities;

namespace Client.Core.Holders.StratumHolder;

public class StratumHandler<TData> : IHandlerService<TData> where TData : Stratum
{
    private readonly BehaviorSubject<List<TData>> _data = new([]);


    public StratumHandler()
    {
        UpdatedData = _data;
    }

    public IObservable<IReadOnlyList<TData>> UpdatedData { get; }

    public Task AddAsync(TData data)
    {
        _data.OnNext([.._data.Value, data]);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id)
    {
        var itemToRemove = _data.Value.FirstOrDefault(data => data.Id == id);

        if (itemToRemove is null)
            return Task.CompletedTask;

        _data.Value.Remove(itemToRemove);
        _data.OnNext(_data.Value);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TData data)
    {
        var currentList = _data.Value;

        var index = currentList.FindIndex(s => s.Id == data.Id);
        if (index == -1)
            AddAsync(data);

        if (currentList[index].Equals(data))
            return Task.CompletedTask;

        var newList = new List<TData>(currentList) { [index] = data };

        _data.OnNext(newList);
        return Task.CompletedTask;
    }
}