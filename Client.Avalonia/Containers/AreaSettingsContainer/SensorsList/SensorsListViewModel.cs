using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Services.SensorsService;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.AreaSettingsContainer.SensorsList;

public class SensorsListViewModel : ViewModelBase
{
    private readonly ISensorsService _sensorsService;

    public SensorsListViewModel(ISensorsService sensorsService)
    {
        _sensorsService = sensorsService;

        SaveSensorsGridCommand = ReactiveCommand.CreateFromTask(
            SaveSensorsGridAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }

    protected override async Task OnActivation(CompositeDisposable disposables)
    {
        await base.OnActivation(disposables);
        _sensorsService
            .SensorsGrid
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                sensors =>
                {
                    SensorsGrid = sensors;
                    Debug.WriteLine(sensors);
                }
            )
            .DisposeWith(disposables);
    }

    public string StartCoordinateLabel { get; set; } = "Нижняя граница";

    public string EndCoordinateLabel { get; set; } = "Верхняя граница";

    public string SplittingParamsLabel { get; set; } = "Кол-во ячеек";

    [Reactive]
    public SensorsGrid? SensorsGrid { get; private set; }

    public ReactiveCommand<Unit, Unit> SaveSensorsGridCommand { get; }

    private Task SaveSensorsGridAsync()
    {
        _sensorsService.UpdateAsync(SensorsGrid!);
        return Task.CompletedTask;
    }
}