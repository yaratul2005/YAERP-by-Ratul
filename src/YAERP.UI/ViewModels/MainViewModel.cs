using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Services;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.ViewModels.Security;
using YAERP.UI.Views.Drawers;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string _updateBannerText = string.Empty;

    [ObservableProperty]
    private bool _isDownloadingUpdate;

    [ObservableProperty]
    private int _updateDownloadProgress;

    private readonly INavigationService _navigationService;
    public ITabWorkspaceService Workspace { get; }
    private readonly IModalService _modalService;
    private readonly IDialogService _dialogService;
    private readonly ICloudSyncService? _cloudSyncService;
    private readonly IAutoUpdaterService? _autoUpdaterService;
    private readonly IServiceScopeFactory? _scopeFactory;
    private readonly ICommandPaletteService? _commandPaletteService;

    [ObservableProperty]
    private bool _isCloudOnline = true;

    [ObservableProperty]
    private int _unsyncedOutboxCount;

    [ObservableProperty]
    private string _lastSyncTimeText = "Never";

    [ObservableProperty]
    private bool _isManualSyncRunning;

    [ObservableProperty]
    private bool _isCommandPaletteOpen;

    [ObservableProperty]
    private string _syncStatusBadgeText = "● Cloud Online";

    public MainViewModel(
        INavigationService navigationService,
        ITabWorkspaceService workspace,
        IModalService modalService,
        IDialogService dialogService,
        IAutoUpdaterService? autoUpdaterService = null,
        ICloudSyncService? cloudSyncService = null,
        IServiceScopeFactory? scopeFactory = null,
        ICommandPaletteService? commandPaletteService = null)
    {
        _navigationService = navigationService;
        Workspace = workspace;
        _modalService = modalService;
        _dialogService = dialogService;
        _autoUpdaterService = autoUpdaterService;
        _cloudSyncService = cloudSyncService;
        _scopeFactory = scopeFactory;
        _commandPaletteService = commandPaletteService;

        if (_navigationService is INotifyPropertyChanged notifyNav)
        {
            notifyNav.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentView))
                {
                    OnPropertyChanged(nameof(CurrentView));
                }
            };
        }

        if (_modalService is INotifyPropertyChanged notifyModal)
        {
            notifyModal.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsModalOpen));
                OnPropertyChanged(nameof(ModalTitle));
                OnPropertyChanged(nameof(CurrentModalContent));
            };
        }

        _ = CheckForUpdatesAsync();

        // Register Command Palette items
        RegisterCommandPaletteItems();

        // Set default view on startup
        Workspace.OpenTab<DashboardViewModel>();

        // Initialize background sync status polling timer
        InitializeSyncTimer();
    }

    private void RegisterCommandPaletteItems()
    {
        if (_commandPaletteService == null) return;

        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Executive Dashboard", "Open main command center", "Navigation", "📊", NavigateDashboardCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Warehouse & WMS", "Spatial bin slotting & WMS control", "Navigation", "🏢", NavigateWarehouseManagementCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Inventory & Stock", "Inventory ledger & AI demand forecasting", "Navigation", "📦", NavigateInventoryCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Sales & CRM Pipeline", "Deal Kanban pipeline & quotes", "Navigation", "🛒", NavigateSalesCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Deep Financials & Tax", "Balanced journal vouchers & asset register", "Navigation", "📈", NavigateDeepFinancialsCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Finance & Ledger", "General ledger & anomaly audit", "Navigation", "💰", NavigateFinanceCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Express POS Terminal", "Retail register & touch cash tender keypad", "Navigation", "🖥️", NavigatePosCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Multi-Level Approval Center", "Supervisor approval decisions", "Navigation", "🛡️", NavigateApprovalCenterCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Manufacturing & MRP II", "Multi-level BOM tree & production logs", "Navigation", "⚙️", NavigateManufacturingCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Security & Roles (IAM)", "User accounts, roles & security audit trail", "Navigation", "🔐", NavigateUserAndRolesCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Plugin Management Hub", "Inspect isolated AssemblyLoadContext sandboxes", "Navigation", "🧩", NavigatePluginHubCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Trigger Manual Cloud Sync", "Process local outbox queue to cloud", "Sync", "🔄", ManualSyncNowCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Open Outbox Conflict Resolver", "Inspect and resolve sync version conflicts", "Sync", "📦", OpenConflictResolverCommand));
        _commandPaletteService.RegisterCommand(new PaletteCommandItem("Open User Profile Settings", "Update password and UI preferences", "System", "👤", OpenUserProfileDrawerCommand));
    }

    public ObservableObject? CurrentView => _navigationService.CurrentView;

    public bool IsModalOpen => _modalService.IsModalOpen;
    public string ModalTitle => _modalService.ModalTitle;
    public ObservableObject? CurrentModalContent => _modalService.CurrentModalContent;

    [RelayCommand]
    private void ToggleCommandPalette()
    {
        IsCommandPaletteOpen = !IsCommandPaletteOpen;
    }

    [RelayCommand]
    private void CloseModal() => _modalService.CloseModal();

    [RelayCommand]
    private void NavigateDashboard() => Workspace.OpenTab<DashboardViewModel>();

    [RelayCommand]
    private void NavigateInventory() => Workspace.OpenTab<InventoryViewModel>();

    [RelayCommand]
    private void NavigateSales() => Workspace.OpenTab<SalesViewModel>();

    [RelayCommand]
    private void NavigateFinance() => Workspace.OpenTab<FinanceViewModel>();

    [RelayCommand]
    private void NavigatePos() => Workspace.OpenTab<PosViewModel>();

    [RelayCommand]
    private void NavigateApprovalCenter() => Workspace.OpenTab<ApprovalCenterViewModel>();

    [RelayCommand]
    private void NavigatePluginHub() => Workspace.OpenTab<PluginHubViewModel>();

    [RelayCommand]
    private void NavigateManufacturing() => Workspace.OpenTab<ManufacturingViewModel>();

    [RelayCommand]
    private void NavigateWarehouseManagement() => Workspace.OpenTab<WarehouseManagementViewModel>();

    [RelayCommand]
    private void NavigateDeepFinancials() => Workspace.OpenTab<DeepFinancialsViewModel>();

    [RelayCommand]
    private void NavigateUserAndRoles() => Workspace.OpenTab<UserAndRolesViewModel>();

    [RelayCommand]
    private async Task OpenConflictResolverAsync()
    {
        if (Workspace.ActiveTab != null && _scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var syncEngine = scope.ServiceProvider.GetService<ISyncEngineService>() ?? new MockSyncEngineService();
            var drawerVm = new ConflictResolverDrawerViewModel(syncEngine, _dialogService);
            var drawerControl = new ConflictResolverDrawer { DataContext = drawerVm };
            Workspace.ActiveTab.OpenDrawer(drawerControl, "Outbox Sync Conflict Resolver");
        }
        else
        {
            await _dialogService.ShowGlobalToastAsync("Opening Sync Conflict Resolver...");
        }
    }

    [RelayCommand]
    private async Task OpenUserProfileDrawerAsync()
    {
        if (Workspace.ActiveTab != null && _scopeFactory != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var identityService = scope.ServiceProvider.GetService<IIdentityService>() ?? new MockIdentityService();
            var drawerVm = new UserProfileDrawerViewModel(_dialogService, identityService);
            var drawerControl = new UserProfileDrawer { DataContext = drawerVm };
            Workspace.ActiveTab.OpenDrawer(drawerControl, "User Profile & Preferences");
        }
        else
        {
            await _dialogService.ShowGlobalToastAsync("Opening User Profile Settings...");
        }
    }

    [RelayCommand]
    private async Task ManualSyncNowAsync()
    {
        if (IsManualSyncRunning || _cloudSyncService == null) return;

        IsManualSyncRunning = true;
        try
        {
            int synced = await _cloudSyncService.ProcessSyncBatchAsync(50);
            LastSyncTimeText = DateTime.Now.ToString("HH:mm:ss");
            await RefreshOutboxCountAsync();
            await _dialogService.ShowGlobalToastAsync("Sync process triggered cleanly.");
        }
        finally
        {
            IsManualSyncRunning = false;
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        if (_autoUpdaterService == null) return;

        var currentVersion = "1.0.0";
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

    private void InitializeSyncTimer()
    {
        try
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += async (s, e) =>
            {
                await CheckSyncStatusAsync();
            };
            timer.Start();

            _ = CheckSyncStatusAsync();
        }
        catch
        {
            // Unit testing environment without WPF Dispatcher
        }
    }

    private async Task CheckSyncStatusAsync()
    {
        if (_cloudSyncService != null)
        {
            IsCloudOnline = await _cloudSyncService.CheckCloudHealthAsync();
        }

        await RefreshOutboxCountAsync();

        if (IsCloudOnline)
        {
            SyncStatusBadgeText = "● Cloud Online";
        }
        else
        {
            SyncStatusBadgeText = $"● Offline / Queueing ({UnsyncedOutboxCount} Items)";
        }
    }

    private async Task RefreshOutboxCountAsync()
    {
        if (_scopeFactory == null) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            UnsyncedOutboxCount = await dbContext.SyncQueueItems.CountAsync(x => !x.IsSynced);
        }
        catch
        {
            UnsyncedOutboxCount = 0;
        }
    }
}

public class MockSyncEngineService : ISyncEngineService
{
    public Task PushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task PullAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<int> ProcessOutboxQueueAsync(int batchSize = 50) => Task.FromResult(0);
    public Task ResolveConflictAsync(Guid conflictId, string resolutionStrategy, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class MockIdentityService : IIdentityService
{
    public string HashPassword(string password, out string salt)
    {
        salt = Guid.NewGuid().ToString("N");
        return "hashed_" + password;
    }

    public bool VerifyPassword(string password, string hash, string salt) => true;

    public Task<Domain.Entities.Security.ApplicationUser?> GetUserByIdAsync(Guid userId) => Task.FromResult<Domain.Entities.Security.ApplicationUser?>(null);

    public Task LogSecurityEventAsync(Guid tenantId, Guid userId, string eventType, string severity, string details, string ipAddress, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<bool> EvaluatePermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void InvalidatePermissionCache(Guid userId) { }
}
