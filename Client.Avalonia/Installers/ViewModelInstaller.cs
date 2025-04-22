using Autofac;
using Client.Avalonia.Containers.AreaSettingsContainer;
using Client.Avalonia.Containers.AreaSettingsContainer.ComputationalDomain;
using Client.Avalonia.Containers.AreaSettingsContainer.SensorsList;
using Client.Avalonia.Containers.AreaSettingsContainer.StratumsList;
using Client.Avalonia.Containers.PlotsContainer;
using Client.Avalonia.Pages.ForwardTaskPage;
using Client.Avalonia.Pages.GravityInversionTaskPage;
using Client.Avalonia.Pages.SettingsPage;
using Client.Avalonia.Windows.MainWindow;
using ReactiveUI;

namespace Client.Avalonia.Installers;

static internal class ViewModelInstaller
{
    public static void RegisterViewModels(this ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().AsSelf().As<IScreen>().SingleInstance();
        builder.RegisterType<AreaSettingsContainerViewModel>();
        builder.RegisterType<StratumsListViewModel>();
        builder.RegisterType<ComputationalDomainSettingsViewModel>();
        builder.RegisterType<PlotsContainerViewModel>();
        builder.RegisterType<ForwardTaskPageViewModel>();
        builder.RegisterType<SettingsPageViewModel>();
        builder.RegisterType<GravityInversionTaskPageViewModel>();
        builder.RegisterType<SensorsListViewModel>();
    }
}