using Autofac;
using Client.Avalonia.Data;
using Client.Core.Holders;
using Client.Core.Holders.StratumHolder;

namespace Client.Avalonia.Installers;

static internal class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumHandler<StratumDto>>().As<IHandlerService<StratumDto>>().SingleInstance();
    }
}