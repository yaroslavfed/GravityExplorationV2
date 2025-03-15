using System.Reactive.Disposables;
using Client.Avalonia.Containers.AreaSettingsContainer;
using Client.Avalonia.Containers.PlotsContainer;
using Client.Avalonia.Properties;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Client.Avalonia.Windows.MainWindow;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public MainWindowViewModel(
        AreaSettingsContainerViewModel areaSettingsContainerViewModel,
        PlotsContainerViewModel plotsContainerViewModel
    )
    {
        AreaSettingsContainerViewModel = areaSettingsContainerViewModel;
        PlotsContainerViewModel = plotsContainerViewModel;
    }

    public RoutingState Router { get; set; } = new();

    public AreaSettingsContainerViewModel AreaSettingsContainerViewModel { get; }

    public PlotsContainerViewModel PlotsContainerViewModel { get; }
}