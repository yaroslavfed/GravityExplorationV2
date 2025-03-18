using System.Reactive;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Properties;
using ReactiveUI;
using Splat;

namespace Client.Avalonia.Pages.ForwardTaskPage;

public class ForwardTaskPageViewModel : ViewModelBase, IRoutableViewModel
{
    public ForwardTaskPageViewModel(IScreen hostScreen)
    {
        HostScreen = hostScreen;

        GotoSettingsPageCommand = ReactiveCommand.CreateFromTask(OpenSettingsPage, outputScheduler: AvaloniaScheduler.Instance);
    }

    private async Task OpenSettingsPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<SettingsPageViewModel>()!;
        await Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(viewModel));
    }

    public string UrlPathSegment => "forward-task-page";

    public IScreen HostScreen { get; }

    public ReactiveCommand<Unit, Unit> GotoSettingsPageCommand { get; }
}