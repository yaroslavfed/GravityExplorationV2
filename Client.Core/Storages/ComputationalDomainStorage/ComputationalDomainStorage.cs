using System.Reactive.Subjects;
using Client.Core.Data;

namespace Client.Core.Storages.ComputationalDomainStorage;

public class ComputationalDomainStorage : IComputationalDomainStorage
{
    private readonly BehaviorSubject<Domain> _domain;

    public ComputationalDomainStorage()
    {
        _domain = new(
            new()
            {
                StartX = 0,
                EndX = 1,
                SplitsXCount = 10,
                StartY = 0,
                EndY = 1,
                SplitsYCount = 10,
                StartZ = 0,
                EndZ = 1,
                SplitsZCount = 10
            }
        );
    }

    public IObservable<Domain> Domain => _domain;

    public Task UpdateAsync(Domain data)
    {
        _domain.OnNext(data);
        return Task.CompletedTask;
    }
}