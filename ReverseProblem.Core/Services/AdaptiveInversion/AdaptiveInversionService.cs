using System.Diagnostics;
using System.Text.Json;
using Common.Data;
using Common.Models;
using DirectTask.Core.Services;
using ReverseProblem.Core.Services.JacobianService;
using ReverseProblem.Core.Services.MeshRefinerService;
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
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions,
        double baseDensity
    )
    {
        var currentMesh = initialMesh;
        double previousFunctional = double.MaxValue;

        for (int iteration = 0; iteration < totalIterations; iteration++)
        {
            Console.WriteLine($"\n== Iteration of adaptive inversion {iteration + 1} ==");

            // 1. Расчёт прямой задачи
            var modelValues = await CalculateForward(currentMesh, sensors, baseDensity);

            // 2. Измеренные значения
            var observedValues = sensors.Select(s => s.Value).ToArray();

            // 3. Расчёт невязки
            var residuals = observedValues.Zip(modelValues, (obs, calc) => obs - calc).ToArray();

            if (residuals.Length == 0)
            {
                Console.WriteLine("End: no residual discrepancy");
                return;
            }

            // 4. Вычисление функционала невязки
            var currentFunctional = residuals.Sum(r => r * r);
            // Console.WriteLine($"Functional of the discrepancy: {currentFunctional:E8}");

            // 5. Проверка критерия останова
            if (currentFunctional < inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("Stop criterion: low functional achieved");
                break;
            }

            if (Math.Abs(previousFunctional - currentFunctional) / previousFunctional < 1e-12)
            {
                Console.WriteLine("Stop criterion: small relative change in functional");
                break;
            }

            previousFunctional = currentFunctional;

            // 6. Построение Якобиана
            var jacobian = _jacobianService.BuildJacobian(currentMesh, sensors);

            // 7. Текущие параметры модели
            var modelParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // 8. Одна итерация метода Гаусса–Ньютона
            var updatedParameters = _gaussNewtonInversionService.Invert(
                modelValues,
                observedValues,
                jacobian,
                modelParameters,
                inversionOptions,
                iteration,
                out double effectiveLambda
            );

            // 8.1. Лог состояния итерации
            Console.WriteLine(
                $"[Iteration {iteration + 1}] Functional: {currentFunctional:E8} | Lambda: {effectiveLambda:E5}"
            );

            // 9. Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];

            Console.WriteLine($"Updated grid: {currentMesh.Cells.Count} cells");

            // 10. Уточнение/объединение сетки (если нужно)
            currentMesh = new() { Cells = _meshRefinerService.RefineOrMergeCellsAdvanced(currentMesh) };
        }

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