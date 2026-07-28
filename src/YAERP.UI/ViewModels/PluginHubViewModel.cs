using System;
using YAERP.UI.Workspace;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Plugins;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.Views.Drawers;

namespace YAERP.UI.ViewModels;

public partial class PluginHubViewModel : TabViewModelBase
{
    private readonly IPluginManager _pluginManager;

    [ObservableProperty]
    private ObservableCollection<IYaerpPlugin> _activePlugins = new();

    [ObservableProperty]
    private string _pluginsFolderPath = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    public PluginHubViewModel(IPluginManager pluginManager)
    {
        Title = "Plugin Hub";
        IconKey = "🧩";
        TabId = "PluginHubViewModel";
        _pluginManager = pluginManager;
        PluginsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        RefreshPlugins();
    }

    public override async Task OnTabActivatedAsync()
    {
        RefreshPlugins();
        await Task.CompletedTask;
    }

    [RelayCommand]
    public void RefreshPlugins()
    {
        try
        {
            StatusMessage = "Scanning for dynamic assembly plugins in ./plugins/ folder...";
            _pluginManager.LoadPlugins(PluginsFolderPath);

            ActivePlugins.Clear();
            foreach (var plugin in _pluginManager.LoadedPlugins)
            {
                ActivePlugins.Add(plugin);
            }

            StatusMessage = ActivePlugins.Count > 0
                ? $"Loaded {ActivePlugins.Count} active assembly plugin(s) via isolated AssemblyLoadContext."
                : $"No dynamic assembly plugins (.dll) found in '{PluginsFolderPath}'. Active AssemblyLoadContext sandbox ready.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading plugins: {ex.Message}";
        }
    }

    [RelayCommand]
    public void OpenPluginLogStreamerDrawer(IYaerpPlugin? plugin)
    {
        var id = plugin?.PluginId ?? "YAERP.Plugin.BarcodeExt";
        var name = plugin?.Name ?? "Barcode & RFID Scanner Interop";
        var ver = plugin?.Version ?? "1.0.4";
        var author = plugin?.Author ?? "Ratul Systems Engineering";

        var drawerVm = new PluginLogStreamerDrawerViewModel(
            id,
            name,
            ver,
            author,
            onCloseRequested: () => CloseDrawer());

        var view = new PluginLogStreamerDrawer { DataContext = drawerVm };
        OpenDrawer(view, $"Plugin Assembly Sandbox Logs - {name}");
    }

    [RelayCommand]
    public void OpenPluginsDirectory()
    {
        try
        {
            if (!Directory.Exists(PluginsFolderPath))
            {
                Directory.CreateDirectory(PluginsFolderPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = PluginsFolderPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open plugins directory: {ex.Message}";
        }
    }
}
