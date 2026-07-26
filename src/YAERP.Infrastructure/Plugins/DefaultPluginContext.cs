using System;
using Microsoft.Extensions.Configuration;
using YAERP.Application.Common.Plugins;

namespace YAERP.Infrastructure.Plugins;

public class DefaultPluginContext : IPluginContext
{
    public DefaultPluginContext(IServiceProvider services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }

    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    public event Action<string, string, Type>? MenuItemRegistered;

    public void RegisterModuleMenuItem(string title, string iconKey, Type viewModelType)
    {
        MenuItemRegistered?.Invoke(title, iconKey, viewModelType);
    }
}
