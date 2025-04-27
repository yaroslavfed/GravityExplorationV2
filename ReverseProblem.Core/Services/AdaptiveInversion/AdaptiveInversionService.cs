using System.Diagnostics;
using System.Text.Json;
using Common.Data;
using DirectTask.Core.Services;
using ReverseProblem.Core.Services.JacobianService;
using ReverseProblem.Core.Services.MeshRefinerService;
using ReverseProblem.GaussNewton.Models;
using ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

namespace ReverseProblem.Core.Services.AdaptiveInversion;

public class AdaptiveInversionService : IAdaptiveInversionService
{
    private readonly IGaussNewtonInversionService _gaussNewtonInversionService;
    private readonly IJacobianService             _jacobianService;
    private readonly IDirectTaskService           _directTaskService;
    private readonly IMeshRefinerService          _meshRefinerService;

    public AdaptiveInversionService(
        IGaussNewtonInversionService gaussNewtonInversionService,
        IJacobianService jacobianService,
        IDirectTaskService directTaskService,
        IMeshRefinerService meshRefinerService
    )
    {
        _gaussNewtonInversionService = gaussNewtonInversionService;
        _jacobianService = jacobianService;
        _directTaskService = directTaskService;
        _meshRefinerService = meshRefinerService;
    }

    public async Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        int totalIterations,
        GaussNewtonInversionOptions inversionOptions,
        double baseDensity
    )
    {
        var currentMesh = initialMesh;

        for (int iteration = 0; iteration < totalIterations; iteration++)
        {
            Console.WriteLine($"\n== Итерация адаптивной инверсии {iteration + 1} ==");

            // === 1. Считаем прямую задачу на текущей сетке ===
            var modelValues = await CalculateForward(currentMesh, sensors, baseDensity);

            // === 2. Извлекаем измеренные значения гравитации ===
            var observedValues = sensors.Select(s => s.Value).ToArray();

            // === 3. Строим Якобиан
            var jacobian = _jacobianService.BuildJacobian(currentMesh, sensors);

            // === 4. Извлекаем текущие плотности
            var initialParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // === 5. Решаем обратную задачу
            var updatedParameters = _gaussNewtonInversionService.Invert(
                modelValues,
                observedValues,
                jacobian,
                initialParameters,
                inversionOptions
            );

            // === 6. Обновляем плотности ячеек
            currentMesh = new() { Cells = _meshRefinerService.RefineOrMergeCellsAdvanced(currentMesh) };

            Console.WriteLine($"→ Сетка после итерации: {currentMesh.Cells.Count} ячеек");
        }

        // TODO: строим график результата обратной задачи, позже заменить 3D на срезы по проекциям
        await ShowPlotAsync(currentMesh);
    }

    private async Task<double[]> CalculateForward(Mesh mesh, List<Sensor> sensors, double baseDensity)
    {
        var anomalyMap = _directTaskService.GetAnomalyMapAsync(mesh, sensors, baseDensity);

        var anomalies = new double[sensors.Count];
        var index = 0;

        await foreach (var anomaly in anomalyMap)
            anomalies[index++] = anomaly.Value;

        return anomalies;
    }

    private async Task ShowPlotAsync(Mesh mesh)
    {
        const string jsonFile = "inverse.json";
        const string pythonPath = "python";
        const string outputImage = "inverse.png";

        await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(mesh));
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
    }
}