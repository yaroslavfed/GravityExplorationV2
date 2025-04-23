using System.Reactive.Subjects;
using Client.Core.Data;

namespace Client.Core.Storages.ComputationalDomainStorage;

internal class ComputationalDomainStorage : IComputationalDomainStorage
{
    private readonly BehaviorSubject<Domain> _domain;

    public ComputationalDomainStorage()
    {
        _domain = new(
            new()
            {
                StartX = -5,
                EndX = 5,
                SplitsXCount = 3,
                StartY = -5,
                EndY = 5,
                SplitsYCount = 3,
                StartZ = -6,
                EndZ = -1,
                SplitsZCount = 3
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