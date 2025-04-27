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
        InverseOptions inverseOptions,
        double baseDensity
    )
    {
        var currentMesh = initialMesh;
        double previousFunctional = double.MaxValue;

        for (int iteration = 0; iteration < totalIterations; iteration++)
        {
            Console.WriteLine($"\n== Iteration of adaptive inversion {iteration + 1} ==");

            // 1. Расчёт прямой задачи
            var anomalyValues = await CalculateForward(currentMesh, sensors, baseDensity);

            // 2. Наблюдённые данные
            var sensorValues = sensors.Select(s => s.Value).ToArray();

            // 3. Получаем невязку
            var residuals = sensorValues.Zip(anomalyValues, (obs, calc) => obs - calc).ToArray();

            // 4. Проверка на наличие невязки
            if (residuals.Length == 0)
            {
                Console.WriteLine("End: there is no residual discrepancy");
                return;
            }

            // 5. Вычисляем функционал невязки
            double currentFunctional = residuals.Sum(r => r * r);
            Console.WriteLine($"The functional of the discrepancy: {currentFunctional:E16}");

            // 6. Проверка критерия останова
            // 6.1. Проверка на достигнутую точность решения
            if (currentFunctional < inverseOptions.FunctionalThreshold)
            {
                Console.WriteLine("End: low functionality has been achieved");
                break;
            }

            // 6.2. Проверка на достаточное уменьшение функционала
            if (Math.Abs(previousFunctional - currentFunctional) < 1e-16)
            {
                Console.WriteLine("End: small change in functionality");
                break;
            }

            previousFunctional = currentFunctional;

            // 7. Построение Якобиана
            var jacobian = _jacobianService.BuildJacobian(currentMesh, sensors);

            // 8. Текущие плотности
            var modelParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // 9. Решаем обратную задачу
            var updatedParameters = _gaussNewtonInversionService.Invert(
                anomalyValues,
                sensorValues,
                jacobian,
                modelParameters,
                inverseOptions,
                iteration
            );

            // 10. Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];

            Console.WriteLine($"The grid after iteration: {currentMesh.Cells.Count} cells");
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