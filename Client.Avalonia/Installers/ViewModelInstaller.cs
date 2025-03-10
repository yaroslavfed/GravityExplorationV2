using Autofac;
using Client.Avalonia.Containers.PlotsContainer;
using Client.Avalonia.Containers.StratumsListContainer;
using Client.Avalonia.Windows.MainWindow;
using ReactiveUI;

namespace Client.Avalonia.Installers;

static internal class ViewModelInstaller
{
    public static void RegisterViewModels(this ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().AsSelf().As<IScreen>().SingleInstance();
        builder.RegisterType<StratumsListContainerViewModel>();
        builder.RegisterType<PlotsContainerViewModel>();
    }
}