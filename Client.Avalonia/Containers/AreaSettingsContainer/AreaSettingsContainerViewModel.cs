using Client.Avalonia.Containers.AreaSettingsContainer.ComputationalDomain;
using Client.Avalonia.Containers.AreaSettingsContainer.SensorsList;
using Client.Avalonia.Containers.AreaSettingsContainer.StratumsList;
using Client.Avalonia.ViewModels;

namespace Client.Avalonia.Containers.AreaSettingsContainer;

public class AreaSettingsContainerViewModel : ViewModelBase
{
    public AreaSettingsContainerViewModel(
        StratumsListViewModel stratumsListViewModel,
        ComputationalDomainSettingsViewModel computationalDomainSettingsViewModel,
        SensorsListViewModel? sensorsListViewModel
    )
    {
        StratumsListViewModel = stratumsListViewModel;
        ComputationalDomainSettingsViewModel = computationalDomainSettingsViewModel;
        SensorsListViewModel = sensorsListViewModel;
    }

    public StratumsListViewModel? StratumsListViewModel { get; }

    public ComputationalDomainSettingsViewModel? ComputationalDomainSettingsViewModel { get; }

    public SensorsListViewModel? SensorsListViewModel { get; }
}