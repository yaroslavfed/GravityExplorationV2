using Common.Data;

namespace Client.Core.Services.SensorsService;

public interface ISensorsService : IEditable<IReadOnlyList<Sensor>>
{
    IObservable<IReadOnlyList<Sensor>> SensorsList { get; }
}