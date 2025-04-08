using Common.Constants;
using Common.Data;
using static System.Math;

namespace DirectTask.Core.Services;

internal class AnomalyService : IAnomalyService
{
    public async IAsyncEnumerable<Sensor> GetAnomalyMapAsync(
        Mesh mesh,
        IReadOnlyList<Sensor> sensors,
        double baseDensity
    )
    {
        foreach (var sensor in sensors)
        {
            double res = 0;
            res += mesh.Cells.Sum(cell => GetAnomaly(sensor, cell, baseDensity));
            yield return sensor with { Value = res };

            await Task.Yield();
        }
    }

    private double GetAnomaly(Sensor sensor, Cell cell, double baseDensity)
    {
        var cellXStart = cell.CenterX - cell.BoundX;
        var cellXEnd = cell.CenterX + cell.BoundX;

        var cellYStart = cell.CenterY - cell.BoundY;
        var cellYEnd = cell.CenterY + cell.BoundY;

        var cellZStart = cell.CenterZ - cell.BoundZ;
        var cellZEnd = cell.CenterZ + cell.BoundZ;

        double result = PhysicalQuantities.GravitationalConstant
                        * (cell.Density - baseDensity)
                        * IntegralCalculation(
                            sensor.X,
                            sensor.Y,
                            sensor.Z,
                            cellXStart,
                            cellXEnd,
                            cellYStart,
                            cellYEnd,
                            cellZStart,
                            cellZEnd
                        );
        return result;
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
        var n = 1E+2;
        var h = (x1 - x0) / n;
        List<double> xs = [];

        var k = x0;
        for (var i = 0; i <= n; i++)
        {
            xs.Add(k);
            k += h;
        }

        double result = 0;
        for (var i = 1; i <= n; i++)
        {
            result += Function(xs[i] - (h / 2));
        }

        result *= h;

        return result;

        double Function(double w)
        {
            return
                Asinh(
                    ((yReceiver - y0)
                     * Sqrt(
                         4 * Pow(zReceiver, 2)
                         - 8 * z1 * zReceiver
                         + 4 * Pow(xReceiver, 2)
                         - 8 * w * xReceiver
                         + 4 * Pow(w, 2)
                         + 4 * Pow(z1, 2)
                     ))
                    / (2 * Pow(zReceiver, 2)
                       - 4 * z1 * zReceiver
                       + 2 * Pow(xReceiver, 2)
                       - 4 * w * xReceiver
                       + 2 * Pow(w, 2)
                       + 2 * Pow(z1, 2))
                )
                - Asinh(
                    ((yReceiver - y1)
                     * Sqrt(
                         4 * Pow(zReceiver, 2)
                         - 8 * z1 * zReceiver
                         + 4 * Pow(xReceiver, 2)
                         - 8 * w * xReceiver
                         + 4 * Pow(w, 2)
                         + 4 * Pow(z1, 2)
                     ))
                    / (2 * Pow(zReceiver, 2)
                       - 4 * z1 * zReceiver
                       + 2 * Pow(xReceiver, 2)
                       - 4 * w * xReceiver
                       + 2 * Pow(w, 2)
                       + 2 * Pow(z1, 2))
                )
                - Asinh(
                    ((yReceiver - y0)
                     * Sqrt(
                         4 * Pow(zReceiver, 2)
                         - 8 * z0 * zReceiver
                         + 4 * Pow(xReceiver, 2)
                         - 8 * w * xReceiver
                         + 4 * Pow(w, 2)
                         + 4 * Pow(z0, 2)
                     ))
                    / (2 * Pow(zReceiver, 2)
                       - 4 * z0 * zReceiver
                       + 2 * Pow(xReceiver, 2)
                       - 4 * w * xReceiver
                       + 2 * Pow(w, 2)
                       + 2 * Pow(z0, 2))
                )
                + Asinh(
                    ((yReceiver - y1)
                     * Sqrt(
                         4 * Pow(zReceiver, 2)
                         - 8 * z0 * zReceiver
                         + 4 * Pow(xReceiver, 2)
                         - 8 * w * xReceiver
                         + 4 * Pow(w, 2)
                         + 4 * Pow(z0, 2)
                     ))
                    / (2 * Pow(zReceiver, 2)
                       - 4 * z0 * zReceiver
                       + 2 * Pow(xReceiver, 2)
                       - 4 * w * xReceiver
                       + 2 * Pow(w, 2)
                       + 2 * Pow(z0, 2))
                );
        }
    }
}