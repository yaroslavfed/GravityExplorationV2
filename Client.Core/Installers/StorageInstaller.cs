using Autofac;
using Client.Core.Storages.ComputationalDomainStorage;
using Client.Core.Storages.SensorsStorage;
using Client.Core.Storages.StratumStorage;

namespace Client.Core.Installers;

public static class StorageInstaller
{
    public static void RegisterClientCoreStorages(this ContainerBuilder builder)
    {
        builder.RegisterType<StratumStorage>().As<IStratumStorage>().SingleInstance();
        builder.RegisterType<ComputationalDomainStorage>().As<IComputationalDomainStorage>().SingleInstance();
        builder.RegisterType<SensorsStorage>().As<ISensorsStorage>().SingleInstance();
    }
}