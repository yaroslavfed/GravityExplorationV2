using System.Numerics;
using Common.Data;
using Common.Models;

namespace ReverseProblem.Core.Services.MeshRefinerService;

public class MeshRefinerService : IMeshRefinerService
{
    public List<Cell> RefineOrMergeCellsAdvanced(
        Mesh mesh,
        List<Sensor> sensors,
        double[] residuals,
        double thresholdRefine,
        double thresholdMerge,
        double maxResidual,
        MeshRefinementOptions refinementOptions,
        SensorsGrid sensorGrid
    )
    {
        var refinedCells = new List<Cell>();

        // Быстрая индексация сенсоров
        var sensorPositions = sensors.Select(s => new Vector3((float)s.X, (float)s.Y, (float)s.Z)).ToArray();

        foreach (var cell in mesh.Cells)
        {
            // 1. Вычисляем локальную невязку вокруг ячейки
            double localResidual = CalculateLocalResidual(cell, sensorPositions, residuals);

            // 2. Условия дробления
            if (localResidual > thresholdRefine
                && localResidual > 0.2 * maxResidual
                && cell.BoundX > refinementOptions.MinCellSizeFraction * (sensorGrid.EndX - sensorGrid.StartX)
                && cell.SubdivisionLevel < refinementOptions.MaxSubdivisionLevel)
            {
                // Дробим ячейку
                refinedCells.AddRange(SplitCell(cell));
            }
            // 3. Условия объединения
            else if (localResidual < thresholdMerge
                     && cell.BoundX * 2
                     <= refinementOptions.MaxCellSizeFraction * (sensorGrid.EndX - sensorGrid.StartX))
            {
                if (CanMergeWithNeighbors(cell, sensorPositions, residuals, thresholdMerge))
                {
                    refinedCells.Add(MergeCell(cell));
                }
                else
                {
                    refinedCells.Add(cell);
                }
            }
            // 4. Если ячейка не подходит ни под дробление, ни под объединение
            else
            {
                refinedCells.Add(cell);
            }
        }

        return refinedCells;
    }

    private static double CalculateLocalResidual(Cell cell, Vector3[] sensorPositions, double[] residuals)
    {
        double influenceRadius = Math.Max(cell.BoundX, Math.Max(cell.BoundY, cell.BoundZ)) * 1.5;

        double sum = 0;
        int count = 0;

        var cellCenter = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);

        for (int i = 0; i < sensorPositions.Length; i++)
        {
            if (Vector3.Distance(cellCenter, sensorPositions[i]) <= influenceRadius)
            {
                sum += Math.Abs(residuals[i]);
                count++;
            }
        }

        if (count == 0)
            return 0;

        return sum / count;
    }

    private static List<Cell> SplitCell(Cell cell)
    {
        var splitCells = new List<Cell>();

        double hx = cell.BoundX / 2;
        double hy = cell.BoundY / 2;
        double hz = cell.BoundZ / 2;

        for (int dx = -1; dx <= 1; dx += 2)
            for (int dy = -1; dy <= 1; dy += 2)
                for (int dz = -1; dz <= 1; dz += 2)
                {
                    splitCells.Add(
                        new Cell
                        {
                            CenterX = cell.CenterX + dx * hx / 2,
                            CenterY = cell.CenterY + dy * hy / 2,
                            CenterZ = cell.CenterZ + dz * hz / 2,
                            BoundX = hx,
                            BoundY = hy,
                            BoundZ = hz,
                            Density = cell.Density,
                            SubdivisionLevel = cell.SubdivisionLevel + 1
                        }
                    );
                }

        return splitCells;
    }

    private static Cell MergeCell(Cell cell)
    {
        return cell with
        {
            BoundX = cell.BoundX * 2,
            BoundY = cell.BoundY * 2,
            BoundZ = cell.BoundZ * 2,
            SubdivisionLevel = Math.Max(0, cell.SubdivisionLevel - 1)
        };
    }

    private static bool CanMergeWithNeighbors(
        Cell cell,
        Vector3[] sensorPositions,
        double[] residuals,
        double thresholdMerge
    )
    {
        double influenceRadius = Math.Max(cell.BoundX, Math.Max(cell.BoundY, cell.BoundZ)) * 2.5;
        var cellCenter = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);

        for (int i = 0; i < sensorPositions.Length; i++)
        {
            if (Vector3.Distance(cellCenter, sensorPositions[i]) <= influenceRadius)
            {
                if (Math.Abs(residuals[i]) > thresholdMerge)
                    return false;
            }
        }

        return true;
    }
}