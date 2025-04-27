using Autofac;
using Client.Core.Services.AnomalyPlotHelper;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.ForwardTaskService;
using Client.Core.Services.InvertTaskService;
using Client.Core.Services.MeshService;
using Client.Core.Services.PlotService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.StratumService;
using Client.Core.Services.TrueModelService;

namespace Client.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterClientCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<TrueModelService>().As<ITrueModelService>().SingleInstance();

        builder.RegisterType<StratumService>().As<IStratumService>();
        builder.RegisterType<ComputationalDomainService>().As<IComputationalDomainService>();
        builder.RegisterType<SensorsService>().As<ISensorsService>();
        builder.RegisterType<MeshService>().As<IMeshService>();
        builder.RegisterType<ForwardTaskService>().As<IForwardTaskService>();
        builder.RegisterType<InvertTaskService>().As<IInvertTaskService>();

        builder.RegisterType<MeshPlotHelper>().As<IMeshPlotHelper>();
        builder.RegisterType<AnomalyPlotHelper>().As<IAnomalyPlotHelper>();
    }
}