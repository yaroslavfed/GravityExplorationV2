using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Containers.AreaSettingsContainer.StratumsList;

public partial class StratumsList : ReactiveUserControl<StratumsListViewModel>
{
    public StratumsList()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}