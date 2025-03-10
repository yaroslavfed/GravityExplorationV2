using Autofac;
using AutoMapper;

namespace Client.Avalonia.Installers;

public static class AutoMapperConfigurationInstaller
{
    public static void RegisterAutoMapperConfiguration(this ContainerBuilder builder)
    {
        builder.Register(_ => new MapperConfiguration(ConfigureMapper)).AsSelf().SingleInstance();
    }

    private static void ConfigureMapper(IMapperConfigurationExpression configuration)
    {
    }
}