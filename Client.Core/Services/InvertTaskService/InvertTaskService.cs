using Client.Core.Services.MeshService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.TrueModelService;
using Common.Data;
using Common.Services;
using ReverseProblem.Core.Services.AdaptiveInversion;
using ReverseProblem.GaussNewton.Models;

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
        var sensors = await _trueModelService.GetTaskSolutionAsync();

        if (sensors is null)
        {
            Console.WriteLine("Ошибка: данные не загружены.");
            return;
        }

        var sensorsGrid = await _sensorsService.GetSensorsGridAsync();
        var baseDensity = await _meshService.GetBaseDensityAsync();

        // TODO: переделать на получение параметров сетки из конфига
        int splitsX = 5;
        int splitsY = 5;
        int splitsZ = 5;
        double depth = 10; // Глубина сетки по Z

        var initialMesh = CreateInitialMeshFromSensorGrid(sensorsGrid, splitsX, splitsY, splitsZ, depth, baseDensity);

        var inversionOptions
            = await ModelFromJsonLoader.LoadOptionsAsync<GaussNewtonInversionOptions>(
                "Properties/inverse_options.json"
            );

        await _adaptiveInversionService.AdaptiveInvertAsync(
            initialMesh,
            sensors,
            totalIterations: 100,
            inversionOptions: inversionOptions,
            baseDensity
        );
    }

    private Mesh CreateInitialMeshFromSensorGrid(
        SensorsGrid grid,
        int splitsX,
        int splitsY,
        int splitsZ,
        double depth,
        double baseDensity = 0
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
                    var centerZ = -(iz + 0.5) * sizeZ; // вниз по Z

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
}