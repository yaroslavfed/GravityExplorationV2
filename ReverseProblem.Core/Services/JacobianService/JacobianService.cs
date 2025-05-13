using System.Collections.Concurrent;
using Common.Data;

namespace ReverseProblem.Core.Services.JacobianService;

/// <summary>
/// Сервис расчёта матрицы Якобиана для сетки ячеек и списка сенсоров.
/// </summary>
public class JacobianService : IJacobianService
{
    /// <summary>
    /// Строит матрицу Якобиана размера [сенсоры × ячейки].
    /// </summary>
    /// <param name="mesh">Сетка ячеек модели.</param>
    /// <param name="sensors">Список сенсоров.</param>
    /// <returns>Двумерная матрица Якобиана.</returns>
    public double[,] BuildJacobian(Mesh mesh, List<Sensor> sensors)
    {
        int m = sensors.Count;
        int n = mesh.Cells.Count;

        var jacobian = new double[m, n];

        Parallel.ForEach(
            Partitioner.Create(0, m),
            () => new double[n], // локальный буфер строки
            (range, _, localRow) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    var sensor = sensors[i];

                    for (int j = 0; j < n; j++)
                    {
                        var cell = mesh.Cells[j];
                        localRow[j] = ComputePartialDerivative(sensor, cell);
                    }

                    for (int j = 0; j < n; j++)
                    {
                        jacobian[i, j] = localRow[j];
                    }
                }

                return localRow;
            },
            _ => { }
        );

        return jacobian;
    }


    /// <summary>
    /// Вычисляет частную производную гравитационного отклика сенсора по плотности ячейки.
    /// </summary>
    /// <param name="sensor">Сенсор.</param>
    /// <param name="cell">Ячейка модели.</param>
    /// <returns>Частная производная ∂g/∂ρ.</returns>
    private double ComputePartialDerivative(Sensor sensor, Cell cell)
    {
        double dx = sensor.X - cell.CenterX;
        double dy = sensor.Y - cell.CenterY;
        double dz = sensor.Z - cell.CenterZ;
        double rSquared = dx * dx + dy * dy + dz * dz;
        double r = Math.Sqrt(rSquared);

        if (r < 1e-6)
            r = 1e-6; // Защита от деления на ноль для близких ячеек

        const double G = 6.67430e-11; // Гравитационная постоянная
        double volume = cell.BoundX * cell.BoundY * cell.BoundZ;

        // Формула влияния массы ячейки на гравитацию (производная по плотности)
        return G * volume / (rSquared * r);
    }
}