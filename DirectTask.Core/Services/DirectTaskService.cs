using Common.Constants;
using Common.Data;
using static System.Math;

namespace DirectTask.Core.Services;

internal class DirectTaskService : IDirectTaskService
{
    public async IAsyncEnumerable<Sensor> GetAnomalyStreamMapAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        double baseDensity
    )
    {
        foreach (var sensor in sensors)
        {
            var res = await Task.Run(() => mesh.Cells.AsParallel().Sum(cell => GetAnomaly(sensor, cell, baseDensity)));
            yield return sensor with { Value = res };
        }
    }

    public List<Sensor> GetAnomalyMapFast(Mesh mesh, IReadOnlyList<Sensor> sensors, double baseDensity)
    {
        var result = new Sensor[sensors.Count];

        Parallel.For(
            0,
            sensors.Count,
            i =>
            {
                var sensor = sensors[i];
                double anomaly = 0;

                foreach (var cell in mesh.Cells)
                {
                    anomaly += GetAnomaly(sensor, cell, baseDensity);
                }

                result[i] = sensor with { Value = anomaly };
            }
        );

        return result.ToList();
    }

    private double IntegralCalculation(
        double xReceiver,
        double yReceiver,
        double zReceiver,
        double x0,
        double x1,
        double y0,
        double y1,
        double z0,
        double z1
    )
    {
        const int n = 25;
        var h = (x1 - x0) / n;
        double result = 0;

        for (int i = 0; i < n; i++)
        {
            double w = x0 + h * (i + 0.5);
            result += Function(w);
        }

        return result * h;

        double Function(double w)
        {
            double x2 = xReceiver * xReceiver;
            double w2 = w * w;

            double F(double y, double z)
            {
                double top = (yReceiver - y)
                             * Sqrt(
                                 4 * zReceiver * zReceiver
                                 - 8 * z * zReceiver
                                 + 4 * x2
                                 - 8 * w * xReceiver
                                 + 4 * w2
                                 + 4 * z * z
                             );
                double bottom = 2 * zReceiver * zReceiver
                                - 4 * z * zReceiver
                                + 2 * x2
                                - 4 * w * xReceiver
                                + 2 * w2
                                + 2 * z * z;

                double divisionResult = top / bottom;
                return double.IsNaN(divisionResult)
                    ? Asinh(0)
                    : Asinh(divisionResult);
            }

            return F(y0, z1) - F(y1, z1) - F(y0, z0) + F(y1, z0);
        }
    }

    private double GetAnomaly(Sensor sensor, Cell cell, double baseDensity)
    {
        var x0 = cell.CenterX - cell.BoundX;
        var x1 = cell.CenterX + cell.BoundX;
        var y0 = cell.CenterY - cell.BoundY;
        var y1 = cell.CenterY + cell.BoundY;
        var z0 = cell.CenterZ - cell.BoundZ;
        var z1 = cell.CenterZ + cell.BoundZ;

        return PhysicalQuantities.GravitationalConstant
               * (cell.Density - baseDensity)
               * IntegralCalculation(sensor.X, sensor.Y, sensor.Z, x0, x1, y0, y1, z0, z1);
    }
}