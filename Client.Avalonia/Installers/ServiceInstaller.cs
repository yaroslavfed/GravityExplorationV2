using Autofac;
using Client.Avalonia.Models;
using Client.Avalonia.Services.StratumHandlerService;
using Client.Core.Services.HandlerService;

namespace Client.Avalonia.Installers;

static internal class ServiceInstaller
{
    public static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumHandlerService>().As<IHandlerService<Stratum>>().SingleInstance();
    }
}