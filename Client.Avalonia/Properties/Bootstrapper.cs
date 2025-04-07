using Autofac;
using Client.Avalonia.Installers;
using Client.Core.Installers;

namespace Client.Avalonia.Properties;

internal class Bootstrapper : BootstrapperBase<Bootstrapper>
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterAutoMapperConfiguration();
        builder.RegisterAutoMapper();

        builder.RegisterServices();
        builder.RegisterStorages();
    }

    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterViewModels();
    }
}