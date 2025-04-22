using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.Properties;
using Client.Core.Services.ReverseProblem;
using Client.Core.Services.TrueModelService;
using Common.Data;
using ReactiveUI;

namespace Client.Avalonia.Pages.GravityInversionTaskPage;

public class GravityInversionTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ITrueModelService  _trueModelService;
    private readonly IAdaptiveInversion _adaptiveInversion;

    public GravityInversionTaskPageViewModel(
        IScreen hostScreen,
        ITrueModelService trueModelService,
        IAdaptiveInversion adaptiveInversion
    )
    {
        _trueModelService = trueModelService;
        _adaptiveInversion = adaptiveInversion;
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

        // Генерируем грубую initialMesh вручную (например, 3×3×2 ячейки в кубе 30×30×20 метров)
        var initialCells = new List<Cell>();
        int nx = 3, ny = 3, nz = 2;
        double dx = 10, dy = 10, dz = 10;

        for (int ix = 0; ix < nx; ix++)
        for (int iy = 0; iy < ny; iy++)
        for (int iz = 0; iz < nz; iz++)
        {
            initialCells.Add(
                new()
                {
                    CenterX = (ix + 0.5) * dx,
                    CenterY = (iy + 0.5) * dy,
                    CenterZ = -(iz + 0.5) * dz, // Под землёй
                    BoundX = dx,
                    BoundY = dy,
                    BoundZ = dz,
                    Density = 0 // Начальное приближение
                }
            );
        }

        var initialMesh = new Mesh { Cells = initialCells };

        await _adaptiveInversion.AdaptiveInvert(
            initialMesh: initialMesh,
            sensors: sensors,
            totalIterations: 20,
            invertIterationsPerLevel: 20,
            lambda: 1e-6,
            densityThreshold: 50
        );
    }
}