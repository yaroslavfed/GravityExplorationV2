using Client.Core.Data;
using Common.Data;

namespace Client.Core.Services.SensorsService;

public interface ISensorsService : IEditable<SensorsGrid>
{
    IObservable<SensorsGrid> SensorsGrid { get; }

    Task<IReadOnlyList<Sensor>> GetSensorsAsync();
}