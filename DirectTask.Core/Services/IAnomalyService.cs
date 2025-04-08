using Common.Data;

namespace DirectTask.Core.Services;

public interface IAnomalyService
{
    public IAsyncEnumerable<Sensor> GetAnomalyMapAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        double baseDensity
    );
}