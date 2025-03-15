using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Containers.AreaSettingsContainer.ComputationalDomain;

public partial class ComputationalDomainSettings : ReactiveUserControl<ComputationalDomainSettingsViewModel>
{
    public ComputationalDomainSettings()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}