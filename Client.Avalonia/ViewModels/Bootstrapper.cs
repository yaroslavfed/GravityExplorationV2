using Autofac;
using Client.Avalonia.Installers;
using Client.Core.Installers;
using DirectTask.Core.Installers;
using ReverseProblem.Core.Installers;
using ReverseProblem.GaussNewton.Installers;

namespace Client.Avalonia.ViewModels;

internal class Bootstrapper : BootstrapperBase<Bootstrapper>
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterAutoMapperConfiguration();
        builder.RegisterAutoMapper();

        builder.RegisterClientCoreServices();
        builder.RegisterClientCoreStorages();

        builder.RegisterDirectTaskCoreServices();
        builder.RegisterReverseProblemCoreServices();
        builder.RegisterReverseProblemGaussNewtonServices();
    }

    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterViewModels();
    }
}