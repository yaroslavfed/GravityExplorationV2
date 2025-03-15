using Client.Avalonia.Containers.AreaSettingsContainer.ComputationalDomain;
using Client.Avalonia.Containers.AreaSettingsContainer.StratumsList;
using Client.Avalonia.Properties;

namespace Client.Avalonia.Containers.AreaSettingsContainer;

public class AreaSettingsContainerViewModel : ViewModelBase
{
    public AreaSettingsContainerViewModel(
        StratumsListViewModel stratumsListViewModel,
        ComputationalDomainSettingsViewModel computationalDomainSettingsViewModel
    )
    {
        StratumsListViewModel = stratumsListViewModel;
        ComputationalDomainSettingsViewModel = computationalDomainSettingsViewModel;
    }

    public StratumsListViewModel? StratumsListViewModel { get; }

    public ComputationalDomainSettingsViewModel? ComputationalDomainSettingsViewModel { get; }
}