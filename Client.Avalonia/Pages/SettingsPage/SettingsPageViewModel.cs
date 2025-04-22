using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Containers.AreaSettingsContainer;
using Client.Avalonia.Containers.PlotsContainer;
using Client.Avalonia.Pages.ForwardTaskPage;
using Client.Avalonia.Properties;
using Client.Core.Services.TrueModelService;
using ReactiveUI;
using Splat;

namespace Client.Avalonia.Pages.SettingsPage;

public class SettingsPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly ITrueModelService _trueModelService;

    public SettingsPageViewModel(
        IScreen hostScreen,
        AreaSettingsContainerViewModel areaSettingsContainerViewModel,
        PlotsContainerViewModel plotsContainerViewModel,
        ITrueModelService trueModelService
    )
    {
        HostScreen = hostScreen;
        AreaSettingsContainerViewModel = areaSettingsContainerViewModel;
        PlotsContainerViewModel = plotsContainerViewModel;
        _trueModelService = trueModelService;

        GotoForwardTaskPageCommand = ReactiveCommand.CreateFromTask(
            OpenForwardTaskPage,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }

    public string UrlPathSegment => "settings-page";

    public IScreen HostScreen { get; }

    public AreaSettingsContainerViewModel? AreaSettingsContainerViewModel { get; }

    public PlotsContainerViewModel? PlotsContainerViewModel { get; }

    public ReactiveCommand<Unit, Unit> GotoForwardTaskPageCommand { get; }

    private async Task OpenForwardTaskPage()
    {
        if (PlotsContainerViewModel!.IsDirty)
        {
            await _trueModelService.SaveTaskSolutionAsync(null);
            PlotsContainerViewModel.IsDirty = false;
        }

        IRoutableViewModel viewModel = Locator.Current.GetService<ForwardTaskPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }
}