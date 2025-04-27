using Common.Data;

namespace ReverseProblem.Core.Services.MeshRefinerService;

public class MeshRefinerService : IMeshRefinerService
{
    public List<Cell> RefineOrMergeCellsAdvanced(Mesh mesh)
    {
        var refinedCells = new List<Cell>();

        // Для поиска соседей — ускорим через индексирование
        var cellDict
            = mesh.Cells.ToDictionary(c => (Math.Round(c.CenterX, 4), Math.Round(c.CenterY, 4),
                                            Math.Round(c.CenterZ, 4))
            );

        var usedCells = new HashSet<(double, double, double)>();

        foreach (var cell in mesh.Cells)
        {
            var key = (Math.Round(cell.CenterX, 4), Math.Round(cell.CenterY, 4), Math.Round(cell.CenterZ, 4));
            if (usedCells.Contains(key))
                continue;

            // Анализ ближайших соседей
            var potentialGroup = new List<Cell> { cell };

            foreach (var dx in new[]
            {
                -1,
                1
            })
                foreach (var dy in new[]
                {
                    -1,
                    1
                })
                    foreach (var dz in new[]
                    {
                        -1,
                        1
                    })
                    {
                        var neighborKey = (Math.Round(cell.CenterX + dx * cell.BoundX, 4),
                                           Math.Round(cell.CenterY + dy * cell.BoundY, 4),
                                           Math.Round(cell.CenterZ + dz * cell.BoundZ, 4));
                        if (cellDict.TryGetValue(neighborKey, out var neighbor))
                        {
                            if (Math.Abs(neighbor.BoundX - cell.BoundX) < 1e-6
                                && Math.Abs(neighbor.BoundY - cell.BoundY) < 1e-6
                                && Math.Abs(neighbor.BoundZ - cell.BoundZ) < 1e-6)
                            {
                                potentialGroup.Add(neighbor);
                            }
                        }
                    }

            if (potentialGroup.Count == 8)
            {
                // Все 8 ячеек рядом — можно объединить!
                double avgX = potentialGroup.Average(c => c.CenterX);
                double avgY = potentialGroup.Average(c => c.CenterY);
                double avgZ = potentialGroup.Average(c => c.CenterZ);
                double avgDensity = potentialGroup.Average(c => c.Density);

                refinedCells.Add(
                    new Cell
                    {
                        CenterX = avgX,
                        CenterY = avgY,
                        CenterZ = avgZ,
                        BoundX = cell.BoundX * 2,
                        BoundY = cell.BoundY * 2,
                        BoundZ = cell.BoundZ * 2,
                        Density = avgDensity
                    }
                );

                // Помечаем все использованные ячейки
                foreach (var used in potentialGroup)
                {
                    var usedKey = (Math.Round(used.CenterX, 4), Math.Round(used.CenterY, 4),
                                   Math.Round(used.CenterZ, 4));
                    usedCells.Add(usedKey);
                }
            }
            else
            {
                // Иначе просто оставляем ячейку
                refinedCells.Add(cell);
            }
        }

        return refinedCells;
    }
}