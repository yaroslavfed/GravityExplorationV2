using System.Reactive.Disposables;
using Client.Avalonia.Containers.PlotsContainer;
using Client.Avalonia.Containers.StratumsListContainer;
using Client.Avalonia.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Client.Avalonia.Windows.MainWindow;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    #region LifeCycle

    protected override void OnActivation(CompositeDisposable disposables)
    {
        InitializeControls();
    }

    #endregion

    #region Properties

    public RoutingState Router { get; set; } = new();

    [Reactive]
    public StratumsListContainerViewModel? ObjectsListContainerViewModel { get; private set; }
    
    [Reactive]
    public PlotsContainerViewModel? PlotsContainerViewModel { get; private set; }

    #endregion

    #region Methods

    private void InitializeControls()
    {
        ObjectsListContainerViewModel = Locator.Current.GetService<StratumsListContainerViewModel>();
        PlotsContainerViewModel = Locator.Current.GetService<PlotsContainerViewModel>();
    }

    #endregion

}