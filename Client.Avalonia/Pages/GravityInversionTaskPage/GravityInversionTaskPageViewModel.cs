using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Services.MeshService;
using Client.Core.Services.ReverseProblem;
using Client.Core.Services.SensorsService;
using Client.Core.Services.TrueModelService;
using Common.Data;
using ReactiveUI;

namespace Client.Avalonia.Pages.GravityInversionTaskPage;

public class GravityInversionTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ITrueModelService  _trueModelService;
    private readonly IAdaptiveInversion _adaptiveInversion;
    private readonly ISensorsService    _sensorsService;
    private readonly IMeshService       _meshService;

    public GravityInversionTaskPageViewModel(
        IScreen hostScreen,
        ITrueModelService trueModelService,
        IAdaptiveInversion adaptiveInversion,
        ISensorsService sensorsService,
        IMeshService meshService
    )
    {
        _trueModelService = trueModelService;
        _adaptiveInversion = adaptiveInversion;
        _sensorsService = sensorsService;
        _meshService = meshService;
        HostScreen = hostScreen;

        CalculateInversionTaskCommand = ReactiveCommand.CreateFromTask(
            CalculateInversionTask,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }

    protected override async Task OnActivation(CompositeDisposable disposables)
    {
        await base.OnActivation(disposables);
        CalculateInversionTaskCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "gravity-inversion-task-page";

    public IScreen HostScreen { get; }

    private ReactiveCommand<Unit, Unit> CalculateInversionTaskCommand { get; }

    private async Task CalculateInversionTask()
    {
        var sensors = await _trueModelService.GetTaskSolutionAsync();

        if (sensors is null)
        {
            Console.WriteLine("Ошибка: данные не загружены.");
            return;
        }

        var sensorsGrid = await _sensorsService.GetSensorsGridAsync();
        var baseDensity = await _meshService.GetBaseDensityAsync();

        // TODO: Параметры сетки
        int splitsX = 4;
        int splitsY = 4;
        int splitsZ = 2;
        double depth = 20; // Глубина сетки по Z

        var initialMesh = CreateInitialMeshFromSensorGrid(sensorsGrid, splitsX, splitsY, splitsZ, depth, baseDensity);

        await _adaptiveInversion.AdaptiveInvert(
            initialMesh: initialMesh,
            sensors: sensors,
            totalIterations: 100000,
            lambda: 1e-8
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