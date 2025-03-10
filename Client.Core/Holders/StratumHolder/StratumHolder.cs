using System.Reactive.Subjects;
using Client.Core.Data.Entities;

namespace Client.Core.Holders.StratumHolder;

public class StratumHolder : IHolderService<Stratum>
{
    private readonly BehaviorSubject<List<Stratum>> _stratum = new([]);


    public StratumHolder()
    {
        DataList = _stratum;
    }

    public IObservable<IReadOnlyList<Stratum>> DataList { get; }

    public Task Add(Stratum data)
    {
        _stratum.OnNext([.._stratum.Value, data]);

        return Task.CompletedTask;
    }

    public Task Remove(Guid id)
    {
        var itemToRemove = _stratum.Value.FirstOrDefault(data => data.Id == id);

        if (itemToRemove is null)
            return Task.CompletedTask;

        _stratum.Value.Remove(itemToRemove);
        _stratum.OnNext(_stratum.Value);

        return Task.CompletedTask;
    }

    public Task Update(Stratum data)
    {
        var currentList = _stratum.Value;

        var index = currentList.FindIndex(s => s.Id == data.Id);
        if (index == -1)
            Add(data);

        if (currentList[index].Equals(data))
            return Task.CompletedTask;

        var newList = new List<Stratum>(currentList) { [index] = data };

        _stratum.OnNext(newList);
        return Task.CompletedTask;
    }
}