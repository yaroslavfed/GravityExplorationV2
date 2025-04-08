using Autofac;
using Client.Avalonia.Installers;
using Client.Core.Installers;
using DirectTask.Core.Installers;

namespace Client.Avalonia.Properties;

internal class Bootstrapper : BootstrapperBase<Bootstrapper>
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterAutoMapperConfiguration();
        builder.RegisterAutoMapper();

        builder.RegisterClientCoreServices();
        builder.RegisterClientCoreStorages();

        builder.RegisterDirectTaskCoreServices();
    }

    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterViewModels();
    }
}