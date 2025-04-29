using Autofac;
using ReverseProblem.GaussNewton.Services.GaussNewtonInversionService;

namespace ReverseProblem.GaussNewton.Installers;

public static class ServiceInstaller
{
    public static void RegisterReverseProblemGaussNewtonServices(this ContainerBuilder builder)
    {
        builder.RegisterType<GaussNewtonInversionService>().As<IGaussNewtonInversionService>();
    }
}