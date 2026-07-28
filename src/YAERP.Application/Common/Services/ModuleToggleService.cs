using System.Collections.Concurrent;

namespace YAERP.Application.Common.Services;

public class ModuleToggleService
{
    private readonly ConcurrentDictionary<string, bool> _toggles = new();

    public void EnableModule(string moduleName)
    {
        _toggles.AddOrUpdate(moduleName, true, (_, _) => true);
    }

    public void DisableModule(string moduleName)
    {
        _toggles.AddOrUpdate(moduleName, false, (_, _) => false);
    }

    public bool IsModuleEnabled(string moduleName)
    {
        return _toggles.TryGetValue(moduleName, out bool isEnabled) && isEnabled;
    }
}
