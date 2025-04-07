using Autofac;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.PlotService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.StratumService;

namespace Client.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumService>().As<IStratumService>();
        builder.RegisterType<ComputationalDomainService>().As<IComputationalDomainService>();
        builder.RegisterType<SensorsService>().As<ISensorsService>();

        builder.RegisterType<PlotService>().As<IPlotService>();
    }
}