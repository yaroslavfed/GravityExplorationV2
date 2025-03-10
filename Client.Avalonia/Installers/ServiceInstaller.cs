using Autofac;
using Client.Core.Data.Entities;
using Client.Core.Holders;
using Client.Core.Holders.StratumHolder;
using Client.Core.Services.StratumService;

namespace Client.Avalonia.Installers;

static internal class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumService>().As<IStratumService>();
        builder.RegisterType<StratumHolder>().As<IHolderService<Stratum>>().SingleInstance();
    }
}