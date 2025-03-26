using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Properties;
using Client.Core.Services.SensorsService;
using Common.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Client.Avalonia.Pages.ForwardTaskPage;

public class ForwardTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ISensorsService _sensorsService;

    public ForwardTaskPageViewModel(IScreen hostScreen, ISensorsService sensorsService)
    {
        HostScreen = hostScreen;
        _sensorsService = sensorsService;

        GotoSettingsPageCommand = ReactiveCommand.CreateFromTask(
            OpenSettingsPage,
            outputScheduler: AvaloniaScheduler.Instance
        );
        
        
    }

    protected override async void OnActivation(CompositeDisposable disposables)
    {
        try
        {
            base.OnActivation(disposables);
            SensorsList = await _sensorsService.GetAllAsync();
        } catch (Exception e)
        {
            // ReSharper disable once AsyncVoidMethod
            throw new("Failed to get all sensors", e);
        }
    }

    public string UrlPathSegment => "forward-task-page";

    public IScreen HostScreen { get; }

    [Reactive]
    public IReadOnlyList<Sensor> SensorsList { get; set; }

    public ReactiveCommand<Unit, Unit> GotoSettingsPageCommand { get; }

    private async Task OpenSettingsPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<SettingsPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }
}