using Client.Core.Data;

namespace Client.Core.Services.StratumService;

public interface IStratumService : IEditable<Stratum>, IWriteable<Stratum>
{
    IObservable<IReadOnlyList<Stratum>> StratumsList { get; }
}