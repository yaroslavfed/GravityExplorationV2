using Client.Core.Data;
using Client.Core.Services;

namespace Client.Core.Storages.StratumStorage;

internal interface IStratumStorage :  IEditable<Stratum>, IWriteable<Stratum>
{
    IObservable<IReadOnlyList<Stratum>> StratumList { get; }
}