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

    public IObservable<IReadOnlyList<Sensor>> SensorsList => _sensorsStorage.SensorsList;

    public Task UpdateAsync(IReadOnlyList<Sensor> data) => _sensorsStorage.UpdateAsync(data);
}