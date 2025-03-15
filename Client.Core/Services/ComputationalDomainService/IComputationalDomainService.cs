using Client.Core.Data;

namespace Client.Core.Services.ComputationalDomainService;

public interface IComputationalDomainService : IEditable<Domain>
{
    IObservable<Domain> Domain { get; }
}