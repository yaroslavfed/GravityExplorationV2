using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.ViewModels;
using Client.Core.Services.InvertTaskService;
using Client.Core.Services.MeshService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.TrueModelService;
using Common.Data;
using ReactiveUI;

namespace Client.Avalonia.Pages.GravityInversionTaskPage;

public class GravityInversionTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ITrueModelService  _trueModelService;
    private readonly ISensorsService    _sensorsService;
    private readonly IMeshService       _meshService;
    private readonly IInvertTaskService _invertTaskService;

    public GravityInversionTaskPageViewModel(
        IScreen hostScreen,
        ITrueModelService trueModelService,
        ISensorsService sensorsService,
        IMeshService meshService,
        IInvertTaskService invertTaskService
    )
    {
        _trueModelService = trueModelService;
        _sensorsService = sensorsService;
        _meshService = meshService;
        _invertTaskService = invertTaskService;
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
        await _invertTaskService.CalculateInversionAsync();
    }
}