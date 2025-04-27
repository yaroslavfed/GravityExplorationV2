using Common.Data;

namespace DirectTask.Core.Services;

public interface IDirectTaskService
{
    IAsyncEnumerable<Sensor> GetAnomalyMapAsync(Mesh mesh, IReadOnlyList<Sensor> sensors, double baseDensity);

    double IntegralCalculation(
        double xReceiver,
        double yReceiver,
        double zReceiver,
        double x0,
        double x1,
        double y0,
        double y1,
        double z0,
        double z1
    );
}