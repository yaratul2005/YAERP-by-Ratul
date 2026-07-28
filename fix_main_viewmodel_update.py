import re

with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'r') as f:
    content = f.read()

# Add dependencies
if "IAutoUpdaterService _autoUpdaterService;" not in content:
    content = content.replace("private readonly ICloudSyncService? _cloudSyncService;", "private readonly ICloudSyncService? _cloudSyncService;\n    private readonly IAutoUpdaterService? _autoUpdaterService;")
    content = content.replace("ICloudSyncService? cloudSyncService = null,", "IAutoUpdaterService? autoUpdaterService = null,\n        ICloudSyncService? cloudSyncService = null,")
    content = content.replace("_cloudSyncService = cloudSyncService;", "_autoUpdaterService = autoUpdaterService;\n        _cloudSyncService = cloudSyncService;")

# Add ObservableProperties for the update banner
banner_props = """
    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _updateBannerText = string.Empty;

    [ObservableProperty]
    private bool _isDownloadingUpdate;

    [ObservableProperty]
    private int _updateDownloadProgress;
"""
if "bool _isUpdateAvailable;" not in content:
    content = content.replace("public partial class MainViewModel : ObservableObject\n{", "public partial class MainViewModel : ObservableObject\n{" + banner_props)

# Add logic for CheckForUpdatesAsync
update_logic = """
    private async Task CheckForUpdatesAsync()
    {
        if (_autoUpdaterService == null) return;

        var currentVersion = "1.0.0"; // In real app: Assembly.GetExecutingAssembly().GetName().Version.ToString()
        var manifest = await _autoUpdaterService.CheckForUpdatesAsync(currentVersion);

        if (manifest != null)
        {
            UpdateBannerText = $"🚀 YAERP v{manifest.Version} is available! [{manifest.ReleaseNotes}]";
            IsUpdateAvailable = true;
        }
    }

    [RelayCommand]
    private async Task DownloadUpdateAsync()
    {
        if (_autoUpdaterService == null) return;

        IsDownloadingUpdate = true;

        await _autoUpdaterService.DownloadUpdateAsync("url", progress =>
        {
            UpdateDownloadProgress = progress;
        });

        await _dialogService.ShowGlobalToastAsync("Update downloaded. Restarting application...");
        IsDownloadingUpdate = false;
        IsUpdateAvailable = false;
    }
"""
if "CheckForUpdatesAsync" not in content:
    content = content.replace("private void InitializeSyncTimer()", update_logic + "\n    private void InitializeSyncTimer()")
    # Also call CheckForUpdatesAsync on startup
    content = content.replace("// Set default view on startup", "_ = CheckForUpdatesAsync();\n\n        // Set default view on startup")

with open('src/YAERP.UI/ViewModels/MainViewModel.cs', 'w') as f:
    f.write(content)
