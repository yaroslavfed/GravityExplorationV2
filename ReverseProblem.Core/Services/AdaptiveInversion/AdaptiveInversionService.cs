using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Common.Data;
using Common.Models;
using DirectTask.Core.Services;
using ReverseProblem.Core.Services.JacobianService;
using ReverseProblem.Core.Services.MeshRefinerService;
using ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

namespace ReverseProblem.Core.Services.AdaptiveInversion;

public class AdaptiveInversionService(
    IGaussNewtonInversionService gaussNewtonInversionService,
    IJacobianService jacobianService,
    IDirectTaskService directTaskService,
    IMeshRefinerService meshRefinerService
) : IAdaptiveInversionService
{
    private readonly Stopwatch                         _timer = new();
    private          double                            _initialFunctional;
    private          ConcurrentDictionary<int, double> _functionalList = [];

    public async Task AdaptiveInvertAsync(
        Mesh initialMesh,
        List<Sensor> sensors,
        SensorsGrid sensorsGrid,
        InverseOptions inversionOptions,
        MeshRefinementOptions refinementOptions,
        double baseDensity
    )
    {
        // Запуск расчёта времени
        _timer.Start();

        // Истинные значения
        var observedValues = sensors.Select(s => s.Value).ToArray();
        var currentMesh = initialMesh;

        double currentFunctional = .0;
        double previousFunctional = double.MaxValue;

        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Gauss-Newton inversion: iteration[{iteration + 1}] ==");

            // Расчёт прямой задачи
            var anomalySensors = directTaskService.GetAnomalyMapFast(currentMesh, sensors, baseDensity);
            var modelValues = anomalySensors.Select(s => s.Value).ToArray();

            // Расчёт невязки и вычисление функционала
            currentFunctional = .0;
            for (var i = 0; i < modelValues.Length; i++)
            {
                var residual = observedValues[i] - modelValues[i];
                var weight = 1; // TODO: заменить на применения весов

                currentFunctional += residual * residual * weight * weight;
            }

            // Сохранение начального функционала
            if (iteration == 0)
            {
                _initialFunctional = currentFunctional;
                Console.WriteLine($"Initial functional was set to: {_initialFunctional:E8}");
            }

            // Проверка на нулевой функционал
            if (Math.Abs(currentFunctional) < 1e-18)
            {
                Console.WriteLine("The model has true parameters");
                break;
            }

            // Проверка на достижение искомого функционала
            var functionalDiv = currentFunctional / _initialFunctional;
            if (!inversionOptions.UseTimeThreshold && functionalDiv <= inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("The desired value of the functional has been achieved");
                break;
            }

            // Лог по итерации
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(
                $"Current functional: {currentFunctional:E8}\t|\tInitial functional: {_initialFunctional:E8}\n"
                + $"(Current functional) / (Initial functional): {functionalDiv}\t|\tFunctional threshold: {inversionOptions.FunctionalThreshold}"
            );
            Console.ResetColor();

            // Проверка на стагнацию функционала
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                if (difference < 0)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");
                Console.ResetColor();

                if (Math.Abs(difference) < inversionOptions.RelativeTolerance)
                {
                    Console.WriteLine("Small relative change in functional");
                }
            }

            // Проверка на выход по времени
            if (inversionOptions.UseTimeThreshold && _timer.Elapsed.TotalMinutes >= inversionOptions.TimeThreshold)
            {
                Console.WriteLine("The time is out");
                break;
            }

            previousFunctional = currentFunctional;

            // 8. Построение A
            Console.WriteLine("Calculating jacobian was started");
            var timer = Stopwatch.StartNew();

            var jacobian = jacobianService.BuildJacobian(currentMesh, sensors);

            timer.Stop();
            Console.WriteLine($"Calculating jacobian was finished in time: {timer.Elapsed.TotalMinutes} minutes");

            // 9. Текущие параметры модели
            var modelParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // 10. Итерация метода Гаусса–Ньютона
            var updatedParameters = gaussNewtonInversionService.Invert(
                modelValues,
                observedValues,
                jacobian,
                modelParameters,
                inversionOptions,
                iteration,
                out var effectiveLambda
            );

            // 11. Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];

            _functionalList.TryAdd(iteration, currentFunctional);
            Console.WriteLine($"Elements: {currentMesh.Cells.Count}");
        }

        _timer.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");
        Console.WriteLine($"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {currentFunctional:E8}");
        Console.ResetColor();

        await ShowPlotAsync(currentMesh, sensors);
        Console.WriteLine("The plot of result has been completed");
    }

    private async Task ShowPlotAsync(Mesh mesh, IReadOnlyList<Sensor> sensors)
    {
        const string jsonFile = "mesh_data.json";
        const string pythonPath = "python";
        const string outputImage = "mesh_data.png";

        await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(mesh));
        var currentDirectory = Directory.GetCurrentDirectory();
        var scriptPath = Path.Combine(currentDirectory, "Scripts\\show_plots_script.py");

        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"{scriptPath} {jsonFile} {outputImage}",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = true
        };

        var process = Process.Start(psi);
        await process?.WaitForExitAsync()!;
    }
}