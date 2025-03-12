using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Client.Avalonia.Models;
using Client.Core.Services.HandlerService;

namespace Client.Avalonia.Services.StratumHandlerService;

public class StratumHandlerService : IHandlerService<Stratum>
{
    private readonly BehaviorSubject<List<Stratum>> _data = new([]);

    public IObservable<IReadOnlyList<Stratum>> UpdatedData => _data.AsObservable();

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