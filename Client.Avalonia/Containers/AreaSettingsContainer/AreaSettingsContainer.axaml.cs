using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Containers.AreaSettingsContainer;

public partial class AreaSettingsContainer : ReactiveUserControl<AreaSettingsContainerViewModel>
{
    public AreaSettingsContainer()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}