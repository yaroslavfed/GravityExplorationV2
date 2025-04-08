using Autofac;
using DirectTask.Core.Services;

namespace DirectTask.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterDirectTaskCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<AnomalyService>().As<IAnomalyService>();
    }
}