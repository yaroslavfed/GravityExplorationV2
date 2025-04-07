using Client.Core.Services;
using Common.Data;

namespace Client.Core.Storages.SensorsStorage;

internal interface ISensorsStorage : IEditable<IReadOnlyList<Sensor>>
{
    IObservable<IReadOnlyList<Sensor>> SensorsList { get; }
}