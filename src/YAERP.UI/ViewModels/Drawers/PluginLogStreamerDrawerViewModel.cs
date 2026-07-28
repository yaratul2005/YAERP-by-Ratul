using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YAERP.UI.ViewModels.Drawers;

public record PluginLogEntryDto(DateTime Timestamp, string Level, string LogMessage);

public partial class PluginLogStreamerDrawerViewModel : ObservableObject
{
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private string _pluginId = "YAERP.Plugin.BarcodeScannerExt";

    [ObservableProperty]
    private string _pluginName = "Barcode Scanner & RFID Extension";

    [ObservableProperty]
    private string _version = "1.0.4";

    [ObservableProperty]
    private string _author = "Ratul Systems Engineering";

    [ObservableProperty]
    private string _status = "Active Sandbox Context";

    public ObservableCollection<PluginLogEntryDto> StreamedLogs { get; } = new();

    public PluginLogStreamerDrawerViewModel(
        string pluginId,
        string pluginName,
        string version,
        string author,
        Action? onCloseRequested = null)
    {
        _pluginId = pluginId;
        _pluginName = pluginName;
        _version = version;
        _author = author;
        _onCloseRequested = onCloseRequested;

        LoadSampleLogs();
    }

    private void LoadSampleLogs()
    {
        StreamedLogs.Clear();
        StreamedLogs.Add(new PluginLogEntryDto(DateTime.UtcNow.AddMinutes(-12), "INFO", $"[Isolated AssemblyLoadContext] Loading assembly '{PluginId}.dll' into sandbox."));
        StreamedLogs.Add(new PluginLogEntryDto(DateTime.UtcNow.AddMinutes(-10), "INFO", "Initializing plugin context & registering custom navigation items."));
        StreamedLogs.Add(new PluginLogEntryDto(DateTime.UtcNow.AddMinutes(-5), "DEBUG", "Serilog logger attached to isolated AppDomain stream."));
        StreamedLogs.Add(new PluginLogEntryDto(DateTime.UtcNow.AddMinutes(-1), "INFO", "Plugin heartbeat check OK. Zero memory leaks detected."));
    }

    [RelayCommand]
    private void Close()
    {
        _onCloseRequested?.Invoke();
    }
}
