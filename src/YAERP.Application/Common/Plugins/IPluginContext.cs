using System;
using Microsoft.Extensions.Configuration;

namespace YAERP.Application.Common.Plugins;

public interface IPluginContext
{
    IServiceProvider Services { get; }
    IConfiguration Configuration { get; }
    void RegisterModuleMenuItem(string title, string iconKey, Type viewModelType);
}
