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

    public async Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        SensorsGrid sensorsGrid,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions,
        double baseDensity
    )
    {
        var currentMesh = initialMesh;
        var previousFunctional = double.MaxValue;
        double initialFunctional = -1;

        _timer.Start();
        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Iteration of adaptive inversion {iteration + 1} ==");

            // 1. Расчёт прямой задачи
            var modelValues = await CalculateForward(currentMesh, sensors, baseDensity);

            // 2. Измеренные значения
            var observedValues = sensors.Select(s => s.Value).ToArray();

            // 3. Расчёт невязки
            var residuals = observedValues.Zip(modelValues, (obs, calc) => obs - calc).ToArray();

            // 3.1. Проверка на пустую невязку
            if (residuals.Length == 0)
            {
                Console.WriteLine("End: no residual discrepancy");
                return;
            }

            // 4. Вычисление функционала невязки
            var currentFunctional = residuals.Sum(r => r * r);
            Console.WriteLine($"Current functional: {currentFunctional:E8}");

            // 5. Сохранение начального функционала и вычисление динамического порога сглаживания
            if (initialFunctional < 0)
            {
                initialFunctional = currentFunctional;
                inversionOptions.SmoothingActivationThreshold
                    = initialFunctional * inversionOptions.DynamicSmoothingActivationFraction;

                Console.WriteLine(
                    $"A dynamic threshold for anti-aliasing activation has been set: {inversionOptions.SmoothingActivationThreshold:E5}"
                );
            }

            // 6. Проверка критерия останова
            if (currentFunctional < inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("Stop criterion: low functional achieved");
                break;
            }

            if (Math.Abs(previousFunctional - currentFunctional) / previousFunctional
                < inversionOptions.RelativeTolerance)
            {
                Console.WriteLine("Stop criterion: small relative change in functional");
                break;
            }

            var difference = previousFunctional - currentFunctional;
            Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");

            previousFunctional = currentFunctional;

            // 7. Автоматическое включение сглаживания второго порядка
            if (!inversionOptions.UseTikhonovSecondOrder
                && currentFunctional < inversionOptions.SmoothingActivationThreshold)
            {
                inversionOptions.UseTikhonovSecondOrder = true;
                inversionOptions.Lambda *= 0.3;

                Console.WriteLine("Second-order smoothing is enabled");
                Console.WriteLine($"New value of lambda: {inversionOptions.Lambda:E5}");
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
            Console.WriteLine(
                $"[Iteration {iteration + 1}] Functional: {currentFunctional:E8} | Lambda: {effectiveLambda:E5} | UseTikhonovFirstOrder: {inversionOptions.UseTikhonovFirstOrder} | UseTikhonovSecondOrder: {inversionOptions.UseTikhonovSecondOrder}"
            );

            // 11. Обновляем плотности ячеек
            for (var j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];
            Console.WriteLine($"Updated grid: {currentMesh.Cells.Count} cells");

            // 12. Вычисление динамических порогов дробления/объединения
            double thresholdRefine = refinementOptions.InitialRefineThreshold
                                     * Math.Pow(refinementOptions.RefinementDecay, iteration);
            double thresholdMerge = refinementOptions.InitialMergeThreshold
                                    * Math.Pow(refinementOptions.RefinementDecay, iteration);

            var maxResidual = residuals.Max();

            // 13. Адаптивное уточнение сетки
            // currentMesh = new()
            // {
            //     Cells = _meshRefinerService.RefineOrMergeCellsAdvanced(
            //         currentMesh,
            //         sensors,
            //         residuals,
            //         thresholdRefine,
            //         thresholdMerge,
            //         maxResidual,
            //         refinementOptions,
            //         sensorsGrid
            //     )
            // };
        }

        _timer.Stop();
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");

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