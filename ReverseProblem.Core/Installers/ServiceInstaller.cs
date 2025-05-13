using Autofac;
using ReverseProblem.Core.Services.AdaptiveInversion;
using ReverseProblem.Core.Services.JacobianService;
using ReverseProblem.Core.Services.MeshRefinerService;

namespace ReverseProblem.Core.Installers;

public static class ServiceInstaller
{
    public static void RegisterReverseProblemCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<AdaptiveInversionService>().As<IAdaptiveInversionService>();
        builder.RegisterType<JacobianService>().As<IJacobianService>();
        builder.RegisterType<MeshRefinerService>().As<IMeshRefinerService>();
    }
}