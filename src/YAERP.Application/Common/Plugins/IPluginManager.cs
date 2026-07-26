using System.Collections.Generic;

namespace YAERP.Application.Common.Plugins;

public interface IPluginManager
{
    IReadOnlyList<IYaerpPlugin> LoadedPlugins { get; }
    void LoadPlugins(string pluginsDirectory);
}
