using Client.Core.Data;
using Client.Core.Storages.ComputationalDomainStorage;

namespace Client.Core.Services.ComputationalDomainService;

public class ComputationalDomainService : IComputationalDomainService
{
    private readonly IComputationalDomainStorage _domainStorage;

    public ComputationalDomainService(IComputationalDomainStorage domainStorage)
    {
        _domainStorage = domainStorage;
    }

    public IObservable<Domain> Domain => _domainStorage.Domain;

    public async Task UpdateAsync(Domain data)
    {
        await _domainStorage.UpdateAsync(data);
    }
}