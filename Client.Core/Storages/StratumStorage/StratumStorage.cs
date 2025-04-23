using System.Reactive.Linq;
using System.Reactive.Subjects;
using Client.Core.Data;

namespace Client.Core.Storages.StratumStorage;

internal class StratumStorage : IStratumStorage
{
    private readonly BehaviorSubject<List<Stratum>> _data = new(
        [
            new Stratum
            {
                Id = Guid.NewGuid(),
                Density = 1800,
                StartX = -5,
                EndX = -4,
                StartY = -5,
                EndY = 5,
                StartZ = -6,
                EndZ = -2,
                IsActive = false
            },
            new Stratum
            {
                Id = Guid.NewGuid(),
                Density = 2000,
                StartX = 4,
                EndX = 5,
                StartY = -5,
                EndY = 5,
                StartZ = -6,
                EndZ = -2,
                IsActive = false
            }
        ]
    );

    public IObservable<IReadOnlyList<Stratum>> StratumList => _data.AsObservable();

    public Task AddAsync(Stratum data)
    {
        _data.OnNext(_data.Value.Append(data).ToList());
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id)
    {
        var newList = _data.Value.Where(data => data.Id != id).ToList();
        _data.OnNext(newList);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Stratum data)
    {
        var currentList = _data.Value;

        var index = currentList.FindIndex(s => s.Id == data.Id);
        if (index == -1)
            AddAsync(data);

        var newList = new List<Stratum>(currentList) { [index] = data };

        _data.OnNext(newList);
        return Task.CompletedTask;
    }
}