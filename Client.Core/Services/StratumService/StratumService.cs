using Client.Core.Data.Entities;
using Client.Core.Holders;

namespace Client.Core.Services.StratumService;

public class StratumService : IStratumService
{
    private readonly IHolderService<Stratum> _holderService;

    public StratumService(IHolderService<Stratum> holderService)
    {
        _holderService = holderService;
    }

    public async Task Add(Stratum data)
    {
        await _holderService.Add(data);
    }

    public async Task Update(Stratum data)
    {
        await _holderService.Update(data);
    }

    public async Task Delete(Guid data)
    {
        await _holderService.Remove(data);
    }

    public Task<Stratum> Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Stratum>> GetAll()
    {
        throw new NotImplementedException();
    }
}