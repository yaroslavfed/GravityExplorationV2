using System.Diagnostics;
using System.Text;
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

    private readonly Stopwatch _timer = new();
    private          double    _initialFunctional;

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

    public Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        SensorsGrid sensorsGrid,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions,
        double baseDensity
    )
    {
        // Истинные значения
        var observedValues = sensors.Select(s => s.Value).ToArray();

        var currentMesh = initialMesh;
        double previousFunctional = double.MaxValue;

        _timer.Start();
        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            var log = new StringBuilder();
            log.AppendLine($"\n== Adaptive Inversion Iteration {iteration + 1} ==");

            // Расчёт прямой задачи
            var anomalySensors = _directTaskService.GetAnomalyMapFast(currentMesh, sensors, baseDensity);
            var modelValues = anomalySensors.Select(s => s.Value).ToArray();
            
            // Расчёт невязки и вычисление функционала
            var currentFunctional = .0;
            for (var i = 0; i < modelValues.Length; i++)
            {
                var residual = observedValues[i] - modelValues[i];
                var weight = 1;

                currentFunctional += residual * residual * weight * weight;
            }

            log.AppendLine($"Functional: {currentFunctional:E8}");

            // Сохранение начального функционала
            if (iteration == 0)
            {
                _initialFunctional = currentFunctional;
                log.AppendLine($"Initial functional: {_initialFunctional:E8}");
            }

            // Проверка на нулевой функционал
            if (currentFunctional == 0)
            {
                log.AppendLine("The model has true parameters");
                Console.WriteLine(log.ToString());
                break;
            }

            // Проверка на достижение искомого функционала
            if (currentFunctional / _initialFunctional < inversionOptions.FunctionalThreshold)
            {
                log.AppendLine("The desired value of the functional has been achieved");
                Console.WriteLine(log.ToString());
                break;
            }

            // Проверка на стагнацию функционала
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                log.AppendLine($"Difference between previous functional and current functional: {difference:E8}");

                if (Math.Abs(difference) < inversionOptions.RelativeTolerance)
                {
                    log.AppendLine("Small relative change in functional");
                    Console.WriteLine(log.ToString());
                }
            }

            previousFunctional = currentFunctional;

            // 8. Построение A
            var jacobian = _jacobianService.BuildJacobian(currentMesh, sensors);

            // 9. Текущие параметры модели
            var modelParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // 10. Итерация метода Гаусса–Ньютона (Решение обратной задачи)
            var updatedParameters = _gaussNewtonInversionService.Invert(
                modelValues,
                observedValues,
                jacobian,
                modelParameters,
                inversionOptions,
                iteration,
                out var effectiveLambda
            );

            // 10.1. Лог состояния итерации
            log.AppendLine(
                $"[Iteration {iteration + 1}] Functional: {currentFunctional:E8} | Lambda: {effectiveLambda:E5} | UseTikhonovFirstOrder: {inversionOptions.UseTikhonovFirstOrder} | UseTikhonovSecondOrder: {inversionOptions.UseTikhonovSecondOrder}"
            );

            // 11. Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];
            log.AppendLine($"Cells: {currentMesh.Cells.Count}");

            Console.WriteLine(log.ToString());
        }

        _timer.Stop();
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");

        ShowPlotAsync(currentMesh);
        return Task.CompletedTask;
    }

    private void ShowPlotAsync(Mesh mesh)
    {
        const string jsonFile = "inverse.json";
        const string pythonPath = "python";
        const string outputImage = "inverse.png";

        File.WriteAllText(jsonFile, JsonSerializer.Serialize(mesh));
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
        process?.WaitForExit();
    }
}