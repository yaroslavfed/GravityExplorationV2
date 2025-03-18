using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Client.Avalonia.Pages.SettingsPage;

public partial class SettingsPage : ReactiveUserControl<SettingsPageViewModel>
{
    public SettingsPage()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}