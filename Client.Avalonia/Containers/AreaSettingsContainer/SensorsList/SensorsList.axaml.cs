using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Client.Avalonia.Containers.AreaSettingsContainer.SensorsList;

public partial class SensorsList : ReactiveUserControl<SensorsListViewModel>
{
    public SensorsList()
    {
        InitializeComponent();
    }
}