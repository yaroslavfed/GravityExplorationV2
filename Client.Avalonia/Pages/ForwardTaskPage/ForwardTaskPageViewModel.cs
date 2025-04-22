using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Pages.GravityInversionTaskPage;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Properties;
using Client.Core.Services.AnomalyPlotHelper;
using Client.Core.Services.ForwardTaskService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.TrueModelService;
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
    private readonly ITrueModelService   _trueModelService;

    public ForwardTaskPageViewModel(
        IScreen hostScreen,
        IForwardTaskService forwardTaskService,
        IAnomalyPlotHelper anomalyPlotHelper,
        ISensorsService sensorsService,
        ITrueModelService trueModelService
    )
    {
        HostScreen = hostScreen;
        _forwardTaskService = forwardTaskService;
        _anomalyPlotHelper = anomalyPlotHelper;
        _sensorsService = sensorsService;
        _trueModelService = trueModelService;

        GotoSettingsPageCommand = ReactiveCommand.CreateFromTask(
            OpenSettingsPage,
            outputScheduler: AvaloniaScheduler.Instance
        );

        GotoOpenGravityInversionTaskPageCommand = ReactiveCommand.CreateFromTask(
            OpenGravityInversionTaskPage,
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

    public ReactiveCommand<Unit, Unit> GotoOpenGravityInversionTaskPageCommand { get; }

    private ReactiveCommand<Unit, Unit> LoadAnomaliesCommand { get; }

    private async Task OpenSettingsPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<SettingsPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }

    private async Task OpenGravityInversionTaskPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<GravityInversionTaskPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }

    private async Task LoadAnomaliesAsync()
    {
        var solution = await _trueModelService.GetTaskSolutionAsync();
        if (solution is not null)
        {
            return;
        }

        var totalAnomaliesCount = (await _sensorsService.GetSensorsAsync()).Count;

        SensorsList.Clear();
        LoadingProgress = 0;
        await foreach (var sensor in await _forwardTaskService.GetAnomalyMapAsync())
        {
            SensorsList.Add(sensor);

            var percentStep = totalAnomaliesCount / 1; // TODO: 100% от общего числа, менять по необходимости
            if (percentStep > 0 && SensorsList.Count % percentStep == 0)
            {
                await UpdateGraphAsync();
                LoadingProgress += percentStep;
            }
        }

        await UpdateGraphAsync();
        LoadingProgress = 100;
        IsLoadingInProgress = false;

        await _trueModelService.SaveTaskSolutionAsync(SensorsList);
    }

    private async Task UpdateGraphAsync()
    {
        var outputImage = await _anomalyPlotHelper.GenerateChartAsync(SensorsList.ToList());
        AnomalyImage = new(outputImage);
    }
}