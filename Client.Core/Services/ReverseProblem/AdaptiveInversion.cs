using System.Diagnostics;
using System.Text.Json;
using Common.Data;

namespace Client.Core.Services.ReverseProblem;

/// АДАПТИВНАЯ ИНВЕРСИЯ
public class AdaptiveInversion : IAdaptiveInversion
{
    private readonly IInversionSolver _solver;

    public AdaptiveInversion(IInversionSolver solver)
    {
        _solver = solver;
    }

    public async Task AdaptiveInvert(
        Mesh initialMesh,
        List<Sensor> sensors,
        int totalIterations,
        int invertIterationsPerLevel,
        double lambda,
        double densityThreshold
    )
    {
        var currentMesh = initialMesh;

        for (int i = 0; i < totalIterations; i++)
        {
            Console.WriteLine($"\n== Итерация {i + 1} ==");

            await _solver.Invert(currentMesh, sensors, invertIterationsPerLevel, lambda);

            var refinedCells = RefineCells(currentMesh.Cells, densityThreshold);
            currentMesh = new() { Cells = refinedCells };

#if false
        const string JSON_FILE = "inverse.json";
        const string PYTHON_PATH = "python";
        const string OUTPUT_IMAGE = "inverse.png";

        await File.WriteAllTextAsync(JSON_FILE, JsonSerializer.Serialize(currentMesh));
        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\inverse_chart.py");

        var psi = new ProcessStartInfo
        {
            FileName = PYTHON_PATH,
            Arguments = $"{scriptPath} {JSON_FILE} {OUTPUT_IMAGE}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        var output = await process?.StandardOutput.ReadToEndAsync()!;
        var error = await process?.StandardError.ReadToEndAsync()!;
        await process?.WaitForExitAsync()!;
#endif

            Console.WriteLine($"→ Обновлённая сетка: {currentMesh.Cells.Count} ячеек");
        }
    }

    public List<Cell> RefineCells(IEnumerable<Cell> cells, double threshold)
    {
        var refined = new List<Cell>();

        foreach (var cell in cells)
        {
            if (Math.Abs(cell.Density) < threshold)
            {
                refined.Add(cell);
                continue;
            }

            double hx = cell.BoundX / 2;
            double hy = cell.BoundY / 2;
            double hz = cell.BoundZ / 2;

            for (int dx = -1; dx <= 1; dx += 2)
            for (int dy = -1; dy <= 1; dy += 2)
            for (int dz = -1; dz <= 1; dz += 2)
            {
                refined.Add(
                    new()
                    {
                        CenterX = cell.CenterX + dx * hx / 2,
                        CenterY = cell.CenterY + dy * hy / 2,
                        CenterZ = cell.CenterZ + dz * hz / 2,
                        BoundX = hx,
                        BoundY = hy,
                        BoundZ = hz,
                        Density = cell.Density
                    }
                );
            }
        }

        return refined;
    }
}