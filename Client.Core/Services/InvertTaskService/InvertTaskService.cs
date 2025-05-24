using System.Diagnostics;
using System.Text.Json;
using Client.Core.Services.MeshService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.TrueModelService;
using Common.Data;
using Common.Models;
using Common.Services;
using ReverseProblem.Core.Services.AdaptiveInversion;

namespace Client.Core.Services.InvertTaskService;

public class InvertTaskService : IInvertTaskService
{
    private readonly IAdaptiveInversionService _adaptiveInversionService;
    private readonly ITrueModelService         _trueModelService;
    private readonly ISensorsService           _sensorsService;
    private readonly IMeshService              _meshService;

    public InvertTaskService(
        IAdaptiveInversionService adaptiveInversionService,
        ITrueModelService trueModelService,
        IMeshService meshService,
        ISensorsService sensorsService
    )
    {
        _adaptiveInversionService = adaptiveInversionService;
        _trueModelService = trueModelService;
        _meshService = meshService;
        _sensorsService = sensorsService;
    }

    public async Task CalculateInversionAsync()
    {
        var sensors = await _trueModelService.GetSolutionAsync();

        if (sensors is null)
        {
            Console.WriteLine("Error: Data has not been uploaded.");
            return;
        }

        var sensorsGrid = await _sensorsService.GetSensorsGridAsync();
        var baseDensity = await _meshService.GetBaseDensityAsync();

        var inversionOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<InverseOptions>("Properties/inverse_options.json");

        var refinementOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<MeshRefinementOptions>(
                "Properties/mesh_refinement_options.json"
            );

        var meshOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<InitialMeshOptions>("Properties/initial_mesh_options.json");

        var initialMesh = CreateInitialMeshFromSensorGrid(
            sensorsGrid,
            meshOptions.SplitsX,
            meshOptions.SplitsY,
            meshOptions.SplitsZ,
            meshOptions.Depth,
            baseDensity
        );

        var trueTestMesh = await _meshService.GetMeshAsync();
        //MeshNoiseAdder.AddGaussianNoise(trueTestMesh, 50);

        await _adaptiveInversionService.AdaptiveInvertAsync(
            trueTestMesh,
            sensors,
            sensorsGrid,
            inversionOptions,
            refinementOptions,
            baseDensity
        );
    }

    private Mesh CreateInitialMeshFromSensorGrid(
        SensorsGrid grid,
        int splitsX,
        int splitsY,
        int splitsZ,
        double depth,
        double baseDensity
    )
    {
        var cells = new List<Cell>();

        var sizeX = (grid.EndX - grid.StartX) / splitsX;
        var sizeY = (grid.EndY - grid.StartY) / splitsY;
        var sizeZ = depth / splitsZ;

        for (var ix = 0; ix < splitsX; ix++)
            for (var iy = 0; iy < splitsY; iy++)
                for (var iz = 0; iz < splitsZ; iz++)
                {
                    var centerX = grid.StartX + (ix + 0.5) * sizeX;
                    var centerY = grid.StartY + (iy + 0.5) * sizeY;
                    var centerZ = -(iz + 0.5) * sizeZ;

                    cells.Add(
                        new()
                        {
                            CenterX = centerX,
                            CenterY = centerY,
                            CenterZ = centerZ,
                            BoundX = sizeX,
                            BoundY = sizeY,
                            BoundZ = sizeZ,
                            Density = baseDensity
                        }
                    );
                }

        return new() { Cells = cells };
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