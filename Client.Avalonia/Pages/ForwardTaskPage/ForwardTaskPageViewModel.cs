using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Properties;
using Client.Core.Services.AnomalyPlotHelper;
using Client.Core.Services.ForwardTaskService;
using Client.Core.Services.SensorsService;
using Common.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Client.Avalonia.Pages.ForwardTaskPage;

public class ForwardTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly IForwardTaskService _forwardTaskService;
    private readonly IAnomalyPlotHelper  _anomalyPlotHelper;
    private readonly ISensorsService     _sensorsService;

    public ForwardTaskPageViewModel(
        IScreen hostScreen,
        IForwardTaskService forwardTaskService,
        IAnomalyPlotHelper anomalyPlotHelper,
        ISensorsService sensorsService
    )
    {
        HostScreen = hostScreen;
        _forwardTaskService = forwardTaskService;
        _anomalyPlotHelper = anomalyPlotHelper;
        _sensorsService = sensorsService;

        GotoSettingsPageCommand = ReactiveCommand.CreateFromTask(
            OpenSettingsPage,
            outputScheduler: AvaloniaScheduler.Instance
        );

        LoadAnomaliesCommand = ReactiveCommand.CreateFromTask(
            LoadAnomaliesAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );

        IsLoadingInProgress = true;
    }

    protected override async Task OnActivation(CompositeDisposable disposables)
    {
        await base.OnActivation(disposables);
        LoadAnomaliesCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "forward-task-page";

    public IScreen HostScreen { get; }

    public ObservableCollection<Sensor> SensorsList { get; } = new();

    [Reactive]
    public Bitmap? AnomalyImage { get; set; }

    [Reactive]
    public int LoadingProgress { get; set; }

    [Reactive]
    public bool IsLoadingInProgress { get; set; }

    public ReactiveCommand<Unit, Unit> GotoSettingsPageCommand { get; }

    private ReactiveCommand<Unit, Unit> LoadAnomaliesCommand { get; }

    private async Task OpenSettingsPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<SettingsPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }

    private async Task LoadAnomaliesAsync()
    {
        var totalAnomaliesCount = (await _sensorsService.GetSensorsAsync()).Count;

        SensorsList.Clear();
        LoadingProgress = 0;
        await foreach (var sensor in await _forwardTaskService.GetAnomalyMapAsync())
        {
            SensorsList.Add(sensor);

            var percentStep = totalAnomaliesCount / 10; // 10% от общего числа
            if (percentStep > 0 && SensorsList.Count % percentStep == 0)
            {
                await UpdateGraphAsync();
                LoadingProgress += 10;
            }
        }

        await UpdateGraphAsync();
        LoadingProgress = 100;
        IsLoadingInProgress = false;
    }

    private async Task UpdateGraphAsync()
    {
        var outputImage = await _anomalyPlotHelper.GenerateChartAsync(SensorsList.ToList());
        AnomalyImage = new(outputImage);
    }
}