using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Containers.StratumsListContainer;

public partial class StratumsListContainer : ReactiveUserControl<StratumsListContainerViewModel>
{
    public StratumsListContainer()
    {
        DataContext = this;
        InitializeComponent();
    }
}