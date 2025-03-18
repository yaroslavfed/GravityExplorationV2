using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Pages.ForwardTaskPage;

public partial class ForwardTaskPage : ReactiveUserControl<ForwardTaskPageViewModel>
{
    public ForwardTaskPage()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}