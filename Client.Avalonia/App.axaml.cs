using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Client.Avalonia.Properties;
using Client.Avalonia.Windows.MainWindow;
using ReactiveUI;
using Splat;

namespace Client.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        SetupMainWindow();
        base.OnFrameworkInitializationCompleted();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();
        Bootstrapper.Instance.BuildIoC();
    }

    private void SetupMainWindow()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        MainWindow window = new() { DataContext = Locator.Current.GetService<IScreen>() };

        window.Show();
        desktop.MainWindow = window;
    }
}