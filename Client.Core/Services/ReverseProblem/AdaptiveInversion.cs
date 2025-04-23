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

    public async Task AdaptiveInvert(Mesh initialMesh, List<Sensor> sensors, int totalIterations, double lambda)
    {
        var currentMesh = initialMesh;

        for (int i = 0; i < totalIterations; i++)
        {
            Console.WriteLine($"\n== Iteration {i + 1} ==");

            var residual = await _solver.Invert(currentMesh, sensors, lambda);

            if (residual.Length == 0)
            {
                Console.WriteLine("End of the discrepancy");
                return;
            }
            
            Console.WriteLine($"  Max: {residual.Max()}");
            Console.WriteLine($"  Min: {residual.Min()}");

            var threshold = residual.Max();

            Console.WriteLine($"  Threshold: {threshold}");

            currentMesh = new() { Cells = RefineCellsByResidual(currentMesh, sensors, residual, threshold) };

            Console.WriteLine($"== Updated grid: {currentMesh.Cells.Count} cells ==");
        }

#if true
        const string jsonFile = "inverse.json";
        const string pythonPath = "python";
        const string outputImage = "inverse.png";

        await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(currentMesh));
        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\inverse_chart.py");

        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"{scriptPath} {jsonFile} {outputImage}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        await process?.WaitForExitAsync()!;
#endif
    }

    public List<Cell> RefineCellsByResidual(
        Mesh mesh,
        List<Sensor> sensors,
        double[] residual,
        double residualThreshold,
        double proximity = 1
    )
    {
        var refined = new List<Cell>();
        var errorSensors = sensors
                           .Select((sensor, i) => new { sensor, res = Math.Abs(residual[i]) })
                           .Where(sr => sr.res > residualThreshold)
                           .Select(sr => sr.sensor)
                           .ToList();

        foreach (var cell in mesh.Cells)
        {
            bool nearBadSensor = errorSensors.Any(sensor =>
                                                      Math.Abs(sensor.X - cell.CenterX) < cell.BoundX / 2 + proximity
                                                      && Math.Abs(sensor.Y - cell.CenterY) < cell.BoundY / 2 + proximity
            );

            if (!nearBadSensor)
            {
                refined.Add(cell); // оставить без изменений
                continue;
            }

            // Делим ячейку
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