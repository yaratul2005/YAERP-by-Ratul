using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Plugins;

namespace YAERP.Infrastructure.Plugins;

public class PluginManager : IPluginManager
{
    private readonly List<IYaerpPlugin> _loadedPlugins = new();
    private readonly List<PluginLoadContext> _loadContexts = new();
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PluginManager> _logger;

    public PluginManager(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<PluginManager> logger)
    {
        _services = services;
        _configuration = configuration;
        _logger = logger;
    }

    public IReadOnlyList<IYaerpPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

    public void LoadPlugins(string pluginsDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginsDirectory))
        {
            pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        }

        if (!Directory.Exists(pluginsDirectory))
        {
            Directory.CreateDirectory(pluginsDirectory);
            _logger.LogInformation("Created plugins directory at '{Path}'", pluginsDirectory);
            return;
        }

        var pluginFiles = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories);
        _logger.LogInformation("Scanning {Count} DLL assemblies in plugins directory '{Path}'", pluginFiles.Length, pluginsDirectory);

        var context = new DefaultPluginContext(_services, _configuration);

        foreach (var dllPath in pluginFiles)
        {
            try
            {
                var alc = new PluginLoadContext(dllPath);
                var assembly = alc.LoadFromAssemblyPath(dllPath);

                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IYaerpPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                foreach (var type in pluginTypes)
                {
                    if (Activator.CreateInstance(type) is IYaerpPlugin plugin)
                    {
                        plugin.Initialize(context);
                        _loadedPlugins.Add(plugin);
                        _loadContexts.Add(alc);

                        _logger.LogInformation("Successfully initialized plugin '{PluginName}' v{Version} by {Author} (ID: {PluginId})",
                            plugin.Name, plugin.Version, plugin.Author, plugin.PluginId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dynamic assembly plugin from '{DllPath}'", dllPath);
            }
        }
    }
}
