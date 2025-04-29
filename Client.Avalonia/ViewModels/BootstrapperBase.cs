using System.Reflection;
using Autofac;
using ReactiveUI;
using Splat;
using Splat.Autofac;

namespace Client.Avalonia.ViewModels;

public abstract class BootstrapperBase<TBootstrapper> where TBootstrapper : BootstrapperBase<TBootstrapper>, new()
{
    public static BootstrapperBase<TBootstrapper> Instance { get; } = new TBootstrapper();

    public void BuildIoC()
    {
        var builder = new ContainerBuilder();
        RegisterServices(builder);
        RegisterViewModels(builder);
        RegisterAutofac(builder);
    }

    protected abstract void RegisterServices(ContainerBuilder builder);

    protected abstract void RegisterViewModels(ContainerBuilder builder);

    private static void RegisterAutofac(ContainerBuilder builder)
    {
        var resolver = builder.UseAutofacDependencyResolver();
        resolver.InitializeSplat();
        resolver.RegisterViewsForViewModels(Assembly.GetEntryAssembly()!);
        resolver.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        builder.RegisterInstance(resolver);
        RegisterViewLocator(builder, resolver);
    }

    private static void RegisterViewLocator(ContainerBuilder builder, AutofacDependencyResolver resolver)
    {
        builder.RegisterInstance(ViewLocator.Current).As<IViewLocator>().SingleInstance();

        var container = builder.Build();

        resolver.SetLifetimeScope(container);
    }
}