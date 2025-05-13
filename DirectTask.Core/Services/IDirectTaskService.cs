using Common.Data;

namespace DirectTask.Core.Services;

public interface IDirectTaskService
{
    IAsyncEnumerable<Sensor> GetAnomalyStreamMapAsync(Mesh mesh, IReadOnlyList<Sensor> sensors, double baseDensity);

    List<Sensor> GetAnomalyMapFast(Mesh mesh, IReadOnlyList<Sensor> sensors, double baseDensity);
}