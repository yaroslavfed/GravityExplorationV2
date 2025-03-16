using Autofac;
using Client.Core.Data;
using Client.Core.Services;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.PlotService;
using Client.Core.Services.StratumHandlerService;
using Client.Core.Storages.ComputationalDomainStorage;

namespace Client.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumHandlerService>().As<IHandlerService<Stratum>>().SingleInstance();
        builder.RegisterType<ComputationalDomainService>().As<IComputationalDomainService>();
        builder.RegisterType<PlotService>().As<IPlotService>();

        builder.RegisterType<ComputationalDomainStorage>().As<IComputationalDomainStorage>().SingleInstance();
    }
}