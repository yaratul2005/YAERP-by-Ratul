namespace YAERP.Application.Common.Plugins;

public interface IYaerpPlugin
{
    string PluginId { get; }
    string Name { get; }
    string Version { get; }
    string Author { get; }
    void Initialize(IPluginContext context);
    void Shutdown();
}
