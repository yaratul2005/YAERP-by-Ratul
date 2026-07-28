using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record WarehouseBinDto(Guid Id, string BinCode, string ZoneType, decimal UtilizationPercent, bool IsColdStorage, bool IsLocked);
public record InventoryLotDto(Guid Id, string LotNumber, string ProductName, DateTime ExpirationDate, decimal Quantity);
public record ValuationLayerDto(Guid Id, string ProductName, DateTime LayerDate, decimal RemainingQuantity, decimal UnitCost, decimal TotalValue);

public partial class WarehouseManagementViewModel : TabViewModelBase
{
    private readonly ISlottingOptimizationService _slottingService;
    private readonly IFefoPickingService _fefoService;
    private readonly IInventoryValuationService _valuationService;

    public ObservableCollection<WarehouseBinDto> SpatialBins { get; } = new();
    public ObservableCollection<InventoryLotDto> ExpiringLots { get; } = new();
    public ObservableCollection<ValuationLayerDto> CostLayers { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public WarehouseManagementViewModel(
        ISlottingOptimizationService slottingService,
        IFefoPickingService fefoService,
        IInventoryValuationService valuationService)
    {
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
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await Task.Delay(300); // Simulate network load

            // Mock Bins
            SpatialBins.Clear();
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z1-A01-B01-L01-P01", "Picking", 85.5m, false, false));
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z2-A05-B02-L03-P02", "ColdStorage", 40.0m, true, false));
            SpatialBins.Add(new WarehouseBinDto(Guid.NewGuid(), "Z3-A10-B05-L01-P04", "BulkStorage", 99.9m, false, true)); // Locked

            // Mock Lots
            ExpiringLots.Clear();
            ExpiringLots.Add(new InventoryLotDto(Guid.NewGuid(), "LOT-2023-001", "Organic Milk", DateTime.UtcNow.AddDays(5), 150m));
            ExpiringLots.Add(new InventoryLotDto(Guid.NewGuid(), "LOT-2023-002", "Fresh Eggs", DateTime.UtcNow.AddDays(12), 300m));

            // Mock Cost Layers
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
    private async Task GenerateFefoPickListAsync()
    {
        IsBusy = true;
        try
        {
            // Simulate generation
            await Task.Delay(500);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RunValuationAudit()
    {
        // Audit logic
    }

    [RelayCommand]
    private void LockBin(WarehouseBinDto bin)
    {
        // Lock bin logic
    }
}
