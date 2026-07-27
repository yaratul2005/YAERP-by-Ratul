using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record BomHeaderDto(Guid Id, string AssemblyName, string RevisionCode);
public record WorkOrderDto(Guid Id, string WorkOrderNumber, string Status, decimal TargetQuantity, decimal CompletedQuantity);
public record MrpResultDto(Guid ProductId, string ProductName, decimal GrossRequirement, decimal OnHand, decimal NetRequirement);

public partial class ManufacturingViewModel : TabViewModelBase
{
    private readonly IMrpExplosionService _mrpExplosionService;

    public ObservableCollection<BomHeaderDto> ActiveBoms { get; } = new();
    public ObservableCollection<WorkOrderDto> ActiveWorkOrders { get; } = new();
    public ObservableCollection<MrpResultDto> MrpSchedule { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public ManufacturingViewModel(IMrpExplosionService mrpExplosionService)
    {
        _mrpExplosionService = mrpExplosionService;

        Title = "Manufacturing & MRP";
        IconKey = "⚙️";
        TabId = "ManufacturingViewModel";
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
            // Simulate loading BOMs and WorkOrders
            await Task.Delay(200);

            ActiveBoms.Clear();
            ActiveBoms.Add(new BomHeaderDto(Guid.NewGuid(), "Gaming PC Assembly", "REV-A"));
            ActiveBoms.Add(new BomHeaderDto(Guid.NewGuid(), "Office Desk Oak", "REV-B"));

            ActiveWorkOrders.Clear();
            ActiveWorkOrders.Add(new WorkOrderDto(Guid.NewGuid(), "WO-1001", "Released", 50, 0));
            ActiveWorkOrders.Add(new WorkOrderDto(Guid.NewGuid(), "WO-1002", "InProcess", 100, 45));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunMrpExplosionAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            MrpSchedule.Clear();
            var results = await _mrpExplosionService.RunMrpAsync(DateTime.UtcNow.AddDays(30), CancellationToken.None);

            foreach (var res in results)
            {
                MrpSchedule.Add(new MrpResultDto(res.ProductId, "Product " + res.ProductId.ToString().Substring(0, 4), res.GrossRequirement, res.OnHandStock, res.NetRequirement));
            }

            if (results.Count == 0)
            {
                // Add some dummy data for UI testing since empty DB won't return anything
                MrpSchedule.Add(new MrpResultDto(Guid.NewGuid(), "CPU Core i9", 150, 20, 130));
                MrpSchedule.Add(new MrpResultDto(Guid.NewGuid(), "DDR5 RAM 32GB", 300, 100, 200));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ReleaseWorkOrder(WorkOrderDto wo)
    {
        // Logic to release WO
    }

    [RelayCommand]
    private void RecordProductionOutput(WorkOrderDto wo)
    {
        // Logic to record output
    }
}
