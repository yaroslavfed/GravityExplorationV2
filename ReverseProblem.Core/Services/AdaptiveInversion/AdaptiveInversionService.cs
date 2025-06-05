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

        await WriteModelToFile(currentMesh, "enter_mesh_model");

        var residuals = new double[sensors.Count];
        var currentFunctional = .0;
        var previousFunctional = double.MaxValue;

        for (var iteration = 0; iteration < inversionOptions.MaxIterations; iteration++)
        {
            Console.WriteLine($"\n== Gauss-Newton inversion: iteration[{iteration + 1}] ==");

            // Расчёт прямой задачи
            var anomalySensors = directTaskService.GetAnomalyMapFast(currentMesh, sensors, baseDensity);
            var modelValues = anomalySensors.Select(s => s.Value).ToArray();

            // Вычисляем невязку
            for (int i = 0; i < sensors.Count; i++)
                residuals[i] = observedValues[i] - modelValues[i];

            // Вычисляем функционал
            currentFunctional = residuals.Select(r => r * r).Sum();

            // Сохранение начального функционала
            if (iteration == 0)
            {
                _initialFunctional = currentFunctional;
                Console.WriteLine($"Initial functional was set to: {_initialFunctional:E8}");

                _functionalList.TryAdd(-1, _initialFunctional);
            }

            // Проверка на нулевой функционал
            if (Math.Abs(currentFunctional) < 1e-18)
            {
                Console.WriteLine("The model has true parameters");
                break;
            }

            // Проверка на достижение искомого функционала
            var functionalDiv = currentFunctional / _initialFunctional;
            if (inversionOptions.EnableApproximation && functionalDiv <= inversionOptions.FunctionalThreshold)
            {
                Console.WriteLine("The desired value of the functional has been achieved");
                break;
            }

            // Проверка на выход по времени
            if (inversionOptions.EnableTimer && _timer.Elapsed.TotalMinutes >= inversionOptions.TimeThreshold)
            {
                Console.WriteLine("The time is out");
                break;
            }

            // Проверка на стагнацию функционала
            var iterationStagnation = false;
            if (iteration != 0)
            {
                var difference = previousFunctional - currentFunctional;
                Console.ForegroundColor = difference < 0
                    ? ConsoleColor.DarkRed
                    : ConsoleColor.DarkBlue;
                Console.WriteLine($"Difference between previous functional and current functional: {difference:E8}");
                Console.ResetColor();

                // inversionOptions.UseTikhonovSecondOrder = true;
                if (difference < inversionOptions.RelativeTolerance)
                {
                    Console.WriteLine("Small relative change in functional");
                    iterationStagnation = true;
                    // inversionOptions.UseTikhonovSecondOrder = false;
                }
            }

            // Лог по итерации
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(
                $"Current functional: {currentFunctional:E8}\t|\tInitial functional: {_initialFunctional:E8}\n\r"
                + $"(Current functional) / (Initial functional): {functionalDiv}\t|\tFunctional threshold: {inversionOptions.FunctionalThreshold}\n\r"
                + $"Tikhonov first is enable: {inversionOptions.UseTikhonovFirstOrder}\tTikhonov second is enable: {inversionOptions.UseTikhonovSecondOrder}"
            );
            Console.ResetColor();

            previousFunctional = currentFunctional;

            // Построение A
            Console.WriteLine("Calculating jacobian was started");
            var timer = Stopwatch.StartNew();

            var jacobian = jacobianService.BuildJacobian(currentMesh, sensors);

            timer.Stop();
            Console.WriteLine($"Calculating jacobian was finished in time: {timer.Elapsed.TotalMinutes} minutes");

            // Текущие параметры модели
            var modelParameters = currentMesh.Cells.Select(c => c.Density).ToArray();

            // Итерация метода Гаусса–Ньютона
            var updatedParameters = gaussNewtonInversionService.Invert(
                modelValues,
                observedValues,
                jacobian,
                modelParameters,
                inversionOptions,
                iteration,
                iterationStagnation
            );

            // Обновляем плотности ячеек
            for (int j = 0; j < currentMesh.Cells.Count; j++)
                currentMesh.Cells[j].Density = updatedParameters[j];

            foreach (var cell in currentMesh.Cells)
            {
                if (double.IsNaN(cell.Density) || double.IsInfinity(cell.Density) || cell.Density <= 0)
                    throw new($"Invalid mu detected: {cell.Density}");
            }

            _functionalList.TryAdd(iteration, currentFunctional);

            Console.WriteLine($"Elements before adaptation: {currentMesh.Cells.Count}");

            // Уточняем сетку
            if (inversionOptions.EnableRefining && iteration is not 0 && iteration % 100 is 0)
            {
                var refinedCells = meshRefinerService.RefineOrMergeCellsAdvanced(
                    currentMesh,
                    sensors,
                    residuals,
                    refinementOptions.RefineThreshold,
                    refinementOptions.MergeThreshold,
                    residuals.Max(Math.Abs),
                    refinementOptions,
                    sensorsGrid
                );

                currentMesh = new() { Cells = new List<Cell>(refinedCells) };
            }

            Console.WriteLine($"Elements after adaptation: {currentMesh.Cells.Count}");
        }

        _timer.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Elapsed time: {_timer.Elapsed}");
        Console.WriteLine($"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {currentFunctional:E8}");
        Console.ResetColor();

        await WriteFunctionalToFile(currentFunctional);
        await WriteModelToFile(currentMesh, "exit_mesh_model");

        await ShowPlotAsync(currentMesh);
        Console.WriteLine("The plot of result has been completed");
    }

    private async Task WriteFunctionalToFile(double lastFunctional)
    {
        await using var writer = new StreamWriter($"GaussNewton_test_3_{DateTime.Now.ToShortDateString()}.txt", false);

        await writer.WriteLineAsync($"Elapsed time: {_timer.Elapsed}");
        await writer.WriteLineAsync(
            $"Initial functional: {_initialFunctional:E8}\t|\tLast functional: {lastFunctional:E8}"
        );

        await writer.WriteLineAsync("\nFunctional list:");
        foreach (var functional in _functionalList)
            await writer.WriteLineAsync($"{functional.Key}: {functional.Value:E8}");
    }

    private async Task WriteModelToFile(Mesh mesh, string filename)
    {
        var json = JsonSerializer.Serialize(mesh, new JsonSerializerOptions { WriteIndented = true });

        var filePath = $"{filename}.json";
        await File.WriteAllTextAsync(filePath, json);

        Console.WriteLine($"Модель сохранена в {filePath}");
    }

    private async Task ShowPlotAsync(Mesh mesh)
    {
        const string jsonFile = "exit_mesh_model.json";
        const string pythonPath = "python";
        const string outputImage = "exit_mesh_data.png";

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