using Autofac;
using AutoMapper;

namespace Client.Avalonia.Properties;

public static class AutomapperInstaller
{
    public static void RegisterAutoMapper(this ContainerBuilder builder)
    {
        builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var config = context.Resolve<MapperConfiguration>();

                return config.CreateMapper(context.Resolve);
            })
            .As<IMapper>()
            .InstancePerLifetimeScope();
    }
}