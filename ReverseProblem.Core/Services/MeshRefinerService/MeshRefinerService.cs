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
        var newCells = new List<Cell>();
        var sensorPositions = sensors.Select(s => new Vector3((float)s.X, (float)s.Y, (float)s.Z)).ToArray();

        maxResidual = residuals.Max();
        double absRefine = refinementOptions.RefineThreshold * maxResidual;
        double absMerge = refinementOptions.MergeThreshold * maxResidual;

        int refinedCount = 0, mergedCount = 0, keptCount = 0;

        // Сначала разбиение и пометка ячеек для слияния
        var cellsToKeep = new List<Cell>();
        var cellsToMergeCandidates = new List<Cell>();

        foreach (var cell in mesh.Cells)
        {
            double localResidual = CalculateLocalResidual(cell, sensorPositions, residuals);

            bool canRefine = localResidual > absRefine && cell.SubdivisionLevel < refinementOptions.MaxSubdivisionLevel;
            if (canRefine)
            {
                // Разбиваем ячейку
                newCells.AddRange(SplitCell(cell));
                refinedCount++;
            }
            else
            {
                // Кандидаты на слияние или сохранение
                if (localResidual < absMerge
                    && cell.BoundX * 2 <= refinementOptions.MaxCellSizeFraction * (sensorGrid.EndX - sensorGrid.StartX))
                {
                    cellsToMergeCandidates.Add(cell);
                }
                else
                {
                    cellsToKeep.Add(cell);
                }
            }
        }

        // Найдем группы соседних ячеек-кандидатов для слияния
        var neighborGroups = FindNeighborGroups(cellsToMergeCandidates);

        foreach (var group in neighborGroups)
        {
            // Проверяем для всей группы, можно ли её слить по остаткам
            bool canMergeGroup = group.All(c => CanMergeWithNeighbors(c, sensorPositions, residuals, absMerge));

            if (canMergeGroup && group.Count > 1)
            {
                // Объединяем группу ячеек
                var mergedCell = MergeCells(group);
                newCells.Add(mergedCell);
                mergedCount++;
            }
            else
            {
                // Добавляем ячейки, которые нельзя объединить, как есть
                newCells.AddRange(group);
                keptCount += group.Count;
            }
        }

        // Добавляем ячейки, которые не попали ни в слияние, ни в разбиение
        newCells.AddRange(cellsToKeep);
        keptCount += cellsToKeep.Count;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(
            $"→ Refined: {refinedCount}, Merged: {mergedCount}, Kept: {keptCount}, Total: {newCells.Count}"
        );
        Console.ResetColor();

        return newCells;
    }

    private static double CalculateLocalResidual(Cell cell, Vector3[] sensorPositions, double[] residuals)
    {
        var center = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);
        double sum = 0;
        double totalWeight = 0;
        const double epsilon = 1e-3f;

        for (int i = 0; i < sensorPositions.Length; i++)
        {
            double distance = Vector3.Distance(center, sensorPositions[i]);
            double weight = 1.0 / (distance + epsilon);
            sum += weight * Math.Abs(residuals[i]);
            totalWeight += weight;
        }

        return totalWeight > 0
            ? sum / totalWeight
            : 0;
    }

    private List<Cell> SplitCell(Cell cell)
    {
        var children = new List<Cell>();
        var newSizeX = cell.BoundX / 2.0;
        var newSizeY = cell.BoundY / 2.0;
        var newSizeZ = cell.BoundZ / 2.0;
        var level = cell.SubdivisionLevel + 1;

        for (int ix = 0; ix < 2; ix++)
            for (int iy = 0; iy < 2; iy++)
                for (int iz = 0; iz < 2; iz++)
                {
                    var newCell = new Cell
                    {
                        BoundX = newSizeX,
                        BoundY = newSizeY,
                        BoundZ = newSizeZ,
                        CenterX = cell.CenterX + (ix - 0.5) * newSizeX,
                        CenterY = cell.CenterY + (iy - 0.5) * newSizeY,
                        CenterZ = cell.CenterZ + (iz - 0.5) * newSizeZ,
                        SubdivisionLevel = level,
                        Density = cell.Density
                    };
                    children.Add(newCell);
                }

        return children;
    }

    private Cell MergeCells(List<Cell> children)
    {
        var mergedCell = new Cell
        {
            CenterX = children.Average(c => c.CenterX),
            CenterY = children.Average(c => c.CenterY),
            CenterZ = children.Average(c => c.CenterZ),
            BoundX = children[0].BoundX * 2,
            BoundY = children[0].BoundY * 2,
            BoundZ = children[0].BoundZ * 2,
            SubdivisionLevel = children[0].SubdivisionLevel - 1,
            Density = children[0].Density // Возможно, здесь нужна усреднённая плотность
        };

        return mergedCell;
    }

    private static bool CanMergeWithNeighbors(
        Cell cell,
        Vector3[] sensorPositions,
        double[] residuals,
        double thresholdMerge
    )
    {
        double influenceRadius = Math.Max(cell.BoundX, Math.Max(cell.BoundY, cell.BoundZ)) * 2.5;
        var center = new Vector3((float)cell.CenterX, (float)cell.CenterY, (float)cell.CenterZ);

        for (int i = 0; i < sensorPositions.Length; i++)
        {
            if (Vector3.Distance(center, sensorPositions[i]) <= influenceRadius)
            {
                if (Math.Abs(residuals[i]) > thresholdMerge)
                    return false;
            }
        }

        return true;
    }

    private bool AreNeighbors(Cell a, Cell b)
    {
        bool OverlapX = IntervalsOverlapOrTouch(
            a.CenterX - a.BoundX,
            a.CenterX + a.BoundX,
            b.CenterX - b.BoundX,
            b.CenterX + b.BoundX
        );
        bool OverlapY = IntervalsOverlapOrTouch(
            a.CenterY - a.BoundY,
            a.CenterY + a.BoundY,
            b.CenterY - b.BoundY,
            b.CenterY + b.BoundY
        );
        bool OverlapZ = IntervalsOverlapOrTouch(
            a.CenterZ - a.BoundZ,
            a.CenterZ + a.BoundZ,
            b.CenterZ - b.BoundZ,
            b.CenterZ + b.BoundZ
        );

        int countOverlaps = (OverlapX
                                ? 1
                                : 0)
                            + (OverlapY
                                ? 1
                                : 0)
                            + (OverlapZ
                                ? 1
                                : 0);

        // Считаем соседними, если пересечение хотя бы по 2 осям
        return countOverlaps >= 2;
    }

    private bool IntervalsOverlapOrTouch(double min1, double max1, double min2, double max2)
    {
        return !(max1 < min2 || max2 < min1);
    }

    private List<List<Cell>> FindNeighborGroups(List<Cell> cells)
    {
        var groups = new List<List<Cell>>();
        var visited = new HashSet<Cell>();

        var neighbors = new Dictionary<Cell, List<Cell>>();
        foreach (var cell in cells)
        {
            neighbors[cell] = new List<Cell>();
        }

        for (int i = 0; i < cells.Count; i++)
        {
            for (int j = i + 1; j < cells.Count; j++)
            {
                if (AreNeighbors(cells[i], cells[j]))
                {
                    neighbors[cells[i]].Add(cells[j]);
                    neighbors[cells[j]].Add(cells[i]);
                }
            }
        }

        foreach (var cell in cells)
        {
            if (!visited.Contains(cell))
            {
                var group = new List<Cell>();
                var queue = new Queue<Cell>();
                queue.Enqueue(cell);
                visited.Add(cell);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    group.Add(current);

                    foreach (var n in neighbors[current])
                    {
                        if (!visited.Contains(n))
                        {
                            visited.Add(n);
                            queue.Enqueue(n);
                        }
                    }
                }

                groups.Add(group);
            }
        }

        return groups;
    }
}