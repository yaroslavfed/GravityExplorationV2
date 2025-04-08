using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Client.Avalonia.Pages.ForwardTaskPage;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Client.Avalonia.Windows.MainWindow;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    protected override async Task OnActivation(CompositeDisposable disposables)
    {
        InitializeStartPage();
        await base.OnActivation(disposables);
    }

    [Reactive]
    public RoutingState Router { get; set; } = new();

    private void InitializeStartPage()
    {
        IRoutableViewModel viewModel = Locator.Current.GetService<SettingsPageViewModel>()!;
        Dispatcher.UIThread.InvokeAsync(() => Router.Navigate.Execute(viewModel));
    }
}