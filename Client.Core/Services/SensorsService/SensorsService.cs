using Client.Core.Data;
using Client.Core.Storages.SensorsStorage;
using Common.Data;

namespace Client.Core.Services.SensorsService;

internal class SensorsService : ISensorsService
{
    private readonly ISensorsStorage _sensorsStorage;

    public SensorsService(ISensorsStorage sensorsStorage)
    {
        _sensorsStorage = sensorsStorage;
    }

    public IObservable<SensorsGrid> SensorsGrid => _sensorsStorage.SensorsList;

    public Task<SensorsGrid> GetSensorsGridAsync() => _sensorsStorage.GetSensorsGridAsync();

    public Task<IReadOnlyList<Sensor>> GetSensorsAsync() => _sensorsStorage.GetSensorsAsync();

    public Task UpdateAsync(SensorsGrid data) => _sensorsStorage.UpdateAsync(data);
}