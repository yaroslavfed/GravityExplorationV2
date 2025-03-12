using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Containers.StratumsListContainer;

public partial class StratumsListContainer : ReactiveUserControl<StratumsListContainerViewModel>
{
    public StratumsListContainer()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}