using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Services;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    public ITabWorkspaceService Workspace { get; }
    private readonly IModalService _modalService;
    private readonly ICloudSyncService? _cloudSyncService;
    private readonly IServiceScopeFactory? _scopeFactory;

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
        ICloudSyncService? cloudSyncService = null,
        IServiceScopeFactory? scopeFactory = null)
    {
        _navigationService = navigationService;
        Workspace = workspace;
        _modalService = modalService;
        _cloudSyncService = cloudSyncService;
        _scopeFactory = scopeFactory;

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

        // Set default view on startup
        Workspace.OpenTab<DashboardViewModel>();

        // Initialize background sync status polling timer
        InitializeSyncTimer();
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
    private async Task ManualSyncNowAsync()
    {
        if (IsManualSyncRunning || _cloudSyncService == null) return;

        IsManualSyncRunning = true;
        try
        {
            int synced = await _cloudSyncService.ProcessSyncBatchAsync(50);
            LastSyncTimeText = DateTime.Now.ToString("HH:mm:ss");
            await RefreshOutboxCountAsync();
        }
        finally
        {
            IsManualSyncRunning = false;
        }
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
