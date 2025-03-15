using Client.Core.Data;
using Client.Core.Services;

namespace Client.Core.Storages.ComputationalDomainStorage;

public interface IComputationalDomainStorage : IEditable<Domain>
{
    public IObservable<Domain> Domain { get; }
}