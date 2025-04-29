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
        var currentMesh = initialMesh;
        double previousFunctional = double.MaxValue;
        double initialFunctional = -1;

        _timer.Start();
        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            var log = new StringBuilder();
            log.AppendLine($"\n== Adaptive Inversion Iteration {iteration + 1} ==");

            // 1. Расчёт прямой задачи
            var anomalySensors = _directTaskService.GetAnomalyMapFast(currentMesh, sensors, baseDensity);
            var modelValues = anomalySensors.Select(s => s.Value).ToArray();

            // 2. Измеренные значения
            var observedValues = sensors.Select(s => s.Value).ToArray();

            // 3. Расчёт невязки
            var residuals = observedValues.Zip(modelValues, (obs, calc) => obs - calc).ToArray();

            // 3.1. Проверка на пустую невязку
            if (residuals.Length == 0)
            {
                log.AppendLine("End: no residual discrepancy");
                Console.WriteLine(log.ToString());
                return Task.CompletedTask;
            }

            // 4. Вычисление функционала невязки
            double currentFunctional = residuals.Sum(r => r * r);
            log.AppendLine($"Functional: {currentFunctional:E8}");

            // 5. Сохранение начального функционала и вычисление динамического порога сглаживания
            if (initialFunctional < 0)
            {
                initialFunctional = currentFunctional;
                inversionOptions.SmoothingActivationThreshold
                    = initialFunctional * inversionOptions.DynamicSmoothingActivationFraction;
                log.AppendLine($"→ Smoothing threshold set: {inversionOptions.SmoothingActivationThreshold:E5}");
            }

            // 6. Проверка критерия останова
            if (currentFunctional < inversionOptions.FunctionalThreshold)
            {
                log.AppendLine("Stop criterion: low functional achieved");
                Console.WriteLine(log.ToString());
                break;
            }

            if (currentFunctional < inversionOptions.FunctionalThreshold
                || Math.Abs(previousFunctional - currentFunctional) / previousFunctional
                < inversionOptions.RelativeTolerance)
            {
                log.AppendLine("Stop criterion: small relative change in functional");
                Console.WriteLine(log.ToString());
                break;
            }

            var difference = previousFunctional - currentFunctional;
            if (difference < 0)
            {
                // inversionOptions.Lambda /= 0.1;
                log.AppendLine("Stop criterion: difference < 0");
                Console.WriteLine(log.ToString());
                break;
            }

            log.AppendLine($"Difference between previous functional and current functional: {difference:E8}");

            bool shouldAdapt = previousFunctional / currentFunctional >= refinementOptions.FunctionalImprovementRatio
                               || Math.Abs(previousFunctional - currentFunctional)
                               < refinementOptions.AdaptiveTriggerTolerance;

            previousFunctional = currentFunctional;

            // 7. Автоматическое включение сглаживания второго порядка
            if (!inversionOptions.UseTikhonovSecondOrder
                && currentFunctional < inversionOptions.SmoothingActivationThreshold)
            {
                inversionOptions.UseTikhonovSecondOrder = true;
                inversionOptions.Lambda *= 0.3;
                log.AppendLine("Second-order smoothing enabled");
                log.AppendLine($"New base lambda: {inversionOptions.Lambda:E5}");
            }

            // 8. Построение Якобиана
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

            // 12. Вычисление динамических порогов дробления/объединения
            if (shouldAdapt)
            {
                double thresholdRefine = refinementOptions.InitialRefineThreshold
                                         * Math.Pow(refinementOptions.RefinementDecay, iteration);
                double thresholdMerge = refinementOptions.InitialMergeThreshold
                                        * Math.Pow(refinementOptions.RefinementDecay, iteration);
                double maxResidual = residuals.Max();

                currentMesh = new()
                {
                    Cells = _meshRefinerService.RefineOrMergeCellsAdvanced(
                        currentMesh,
                        sensors,
                        residuals,
                        thresholdRefine,
                        thresholdMerge,
                        maxResidual,
                        refinementOptions,
                        sensorsGrid
                    )
                };

                log.AppendLine($"Cells after refinement: {currentMesh.Cells.Count}");
            }
            else
            {
                log.AppendLine("No refinement triggered this iteration");
            }

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