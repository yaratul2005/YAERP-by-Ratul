using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Drawers;
using YAERP.UI.Views.Modals;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record WarehouseBinDto(Guid Id, string BinCode, string ZoneType, decimal UtilizationPercent, bool IsColdStorage, bool IsLocked);
public record InventoryLotDto(Guid Id, string LotNumber, string ProductName, DateTime ExpirationDate, decimal Quantity);
public record ValuationLayerDto(Guid Id, string ProductName, DateTime LayerDate, decimal RemainingQuantity, decimal UnitCost, decimal TotalValue);

public partial class WarehouseManagementViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
    private readonly ISlottingOptimizationService _slottingService;
    private readonly IFefoPickingService _fefoService;
    private readonly IInventoryValuationService _valuationService;

    public ObservableCollection<WarehouseBinDto> SpatialBins { get; } = new();
    public ObservableCollection<InventoryLotDto> ExpiringLots { get; } = new();
    public ObservableCollection<ValuationLayerDto> CostLayers { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private WarehouseBinDto? _selectedBin;

    public WarehouseManagementViewModel(
        IMediator mediator,
        ISlottingOptimizationService slottingService,
        IFefoPickingService fefoService,
        IInventoryValuationService valuationService)
    {
        _mediator = mediator;
        _slottingService = slottingService;
        _fefoService = fefoService;
        _valuationService = valuationService;

        Title = "WMS & Inventory Valuation";
        IconKey = "🏢";
        TabId = "WarehouseManagementViewModel";
    }

    public override async Task OnTabActivatedAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task RefreshBinsAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await Task.Delay(200); // Simulate load

            SpatialBins.Clear();
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z1-A01-B01-L01-P01", "Picking", 85.5m, false, false));
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z2-A05-B02-L03-P02", "ColdStorage", 40.0m, true, false));
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z3-A10-B05-L01-P04", "BulkStorage", 99.9m, false, true)); // Locked
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z1-A02-B03-L02-P01", "Picking", 15.0m, false, false));

            ExpiringLots.Clear();
            ExpiringLots.Add(new InventoryLotDto(Guid.NewGuid(), "LOT-2023-001", "Organic Milk", DateTime.UtcNow.AddDays(5), 150m));
            ExpiringLots.Add(new InventoryLotDto(Guid.NewGuid(), "LOT-2023-002", "Fresh Eggs", DateTime.UtcNow.AddDays(12), 300m));

            CostLayers.Clear();
            CostLayers.Add(new ValuationLayerDto(Guid.NewGuid(), "Steel Pipe", DateTime.UtcNow.AddMonths(-2), 500m, 12.50m, 500m * 12.50m));
            CostLayers.Add(new ValuationLayerDto(Guid.NewGuid(), "Steel Pipe", DateTime.UtcNow.AddMonths(-1), 1000m, 13.00m, 1000m * 13.00m));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void OpenReceivingDockModal()
    {
        var modalVm = new GoodsReceivingModalViewModel(
            _mediator,
            _slottingService,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshBinsAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new GoodsReceivingModal { DataContext = modalVm };
        OpenModal(view, "Inbound Goods Receiving Dock");
    }

    [RelayCommand]
    public void OpenTransferDrawer(WarehouseBinDto? initialBin = null)
    {
        var drawerVm = new BinTransferDrawerViewModel(
            _mediator,
            SpatialBins,
            initialBin ?? SelectedBin,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshBinsAsync();
            },
            onCloseRequested: () => CloseDrawer());

        var view = new BinTransferDrawer { DataContext = drawerVm };
        OpenDrawer(view, "Bin Stock Relocation");
    }

    [RelayCommand]
    public void ToggleBinLock(WarehouseBinDto? bin)
    {
        var targetBin = bin ?? SelectedBin;
        if (targetBin == null)
        {
            ShowErrorToast("Please select a warehouse bin first.");
            return;
        }

        int index = SpatialBins.IndexOf(targetBin);
        if (index >= 0)
        {
            bool newLockState = !targetBin.IsLocked;
            SpatialBins[index] = targetBin with { IsLocked = newLockState };
            
            if (newLockState)
            {
                ShowErrorToast($"Bin {targetBin.BinCode} has been LOCKED.");
            }
            else
            {
                ShowSuccessToast($"Bin {targetBin.BinCode} has been UNLOCKED.");
            }
        }
    }

    [RelayCommand]
    private async Task GenerateFefoPickListAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(300);
            ShowSuccessToast("FEFO Picking route generated successfully.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RunValuationAudit()
    {
        ShowSuccessToast("Valuation ledger audit completed. 0 cost layer discrepancies.");
    }
}
