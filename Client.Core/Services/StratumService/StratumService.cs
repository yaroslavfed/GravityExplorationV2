using Client.Core.Data;
using Client.Core.Storages.StratumStorage;

namespace Client.Core.Services.StratumService;

internal class StratumService : IStratumService
{
    private readonly IStratumStorage _stratumStorage;

    public StratumService(IStratumStorage stratumStorage)
    {
        _stratumStorage = stratumStorage;
    }

    public IObservable<IReadOnlyList<Stratum>> StratumsList => _stratumStorage.StratumList;

    public async Task UpdateAsync(Stratum data) => await _stratumStorage.UpdateAsync(data);

    public async Task AddAsync(Stratum data) => await _stratumStorage.AddAsync(data);

    public async Task RemoveAsync(Guid id) => await _stratumStorage.RemoveAsync(id);

}