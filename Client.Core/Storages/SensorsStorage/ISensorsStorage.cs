using Client.Core.Data;
using Client.Core.Services;
using Common.Data;

namespace Client.Core.Storages.SensorsStorage;

internal interface ISensorsStorage : IEditable<SensorsGrid>
{
    IObservable<SensorsGrid> SensorsList { get; }

    Task<SensorsGrid> GetSensorsGridAsync();

    Task<IReadOnlyList<Sensor>> GetSensorsAsync();
}