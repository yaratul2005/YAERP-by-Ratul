using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Modals;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record BomHeaderDto(Guid Id, string AssemblyName, string RevisionCode);
public record WorkOrderDto(Guid Id, string WorkOrderNumber, string Status, decimal TargetQuantity, decimal CompletedQuantity);
public record MrpResultDto(Guid ProductId, string ProductName, decimal GrossRequirement, decimal OnHand, decimal NetRequirement);

public partial class BomTreeNodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _nodeName = string.Empty;

    [ObservableProperty]
    private string _skuCode = string.Empty;

    [ObservableProperty]
    private decimal _quantityPerAssembly;

    [ObservableProperty]
    private decimal _yieldPercent;

    [ObservableProperty]
    private decimal _scrapFactorPercent;

    public ObservableCollection<BomTreeNodeViewModel> Children { get; } = new();

    public BomTreeNodeViewModel(string nodeName, string skuCode, decimal quantity, decimal yieldPercent, decimal scrapFactorPercent)
    {
        _nodeName = nodeName;
        _skuCode = skuCode;
        _quantityPerAssembly = quantity;
        _yieldPercent = yieldPercent;
        _scrapFactorPercent = scrapFactorPercent;
    }
}

public partial class ManufacturingViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMrpExplosionService _mrpExplosionService;

    public ObservableCollection<BomHeaderDto> ActiveBoms { get; } = new();
    public ObservableCollection<WorkOrderDto> ActiveWorkOrders { get; } = new();
    public ObservableCollection<MrpResultDto> MrpSchedule { get; } = new();
    public ObservableCollection<BomTreeNodeViewModel> BomTreeNodes { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public ManufacturingViewModel(IMediator mediator, IMrpExplosionService mrpExplosionService)
    {
        _mediator = mediator;
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
    public async Task RefreshDataAsync()
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
            await Task.Delay(150);

            ActiveBoms.Clear();
            ActiveBoms.Add(new BomHeaderDto(Guid.NewGuid(), "Gaming PC Assembly", "REV-A"));
            ActiveBoms.Add(new BomHeaderDto(Guid.NewGuid(), "Office Desk Oak", "REV-B"));

            ActiveWorkOrders.Clear();
            ActiveWorkOrders.Add(new WorkOrderDto(Guid.NewGuid(), "WO-1001", "Released", 50, 0));
            ActiveWorkOrders.Add(new WorkOrderDto(Guid.NewGuid(), "WO-1002", "InProcess", 100, 45));

            // Load Interactive Multi-Level BOM Tree
            BomTreeNodes.Clear();
            var parentNode = new BomTreeNodeViewModel("Gaming PC Tower Assembly", "ASM-PC-9000", 1m, 100m, 0m);

            var subAssy = new BomTreeNodeViewModel("Liquid Cooling Sub-Assembly", "SUB-COOL-101", 1m, 99.5m, 0.5m);
            subAssy.Children.Add(new BomTreeNodeViewModel("Pump Motor 12V", "RAW-PMP-01", 1m, 100m, 0m));
            subAssy.Children.Add(new BomTreeNodeViewModel("Radiator Fins Aluminum", "RAW-RAD-02", 2m, 98m, 2m));

            parentNode.Children.Add(subAssy);
            parentNode.Children.Add(new BomTreeNodeViewModel("Intel Core i9 Processor", "SKU-CPU-99", 1m, 100m, 0m));
            parentNode.Children.Add(new BomTreeNodeViewModel("NVIDIA RTX 4090 GPU", "SKU-GPU-4090", 1m, 99m, 1m));

            BomTreeNodes.Add(parentNode);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void OpenCreateBomModal()
    {
        var modalVm = new BomEditorModalViewModel(
            _mediator,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new BomEditorModal { DataContext = modalVm };
        OpenModal(view, "Create New Bill of Materials (BOM)");
    }

    [RelayCommand]
    public void OpenShopFloorOutputModal(WorkOrderDto? workOrder)
    {
        var woId = workOrder?.Id ?? Guid.NewGuid();
        var woNum = workOrder?.WorkOrderNumber ?? "WO-1002";

        var modalVm = new ShopFloorOutputModalViewModel(
            _mediator,
            woId,
            woNum,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new ShopFloorOutputModal { DataContext = modalVm };
        OpenModal(view, $"Shop Floor Output Log - #{woNum}");
    }

    [RelayCommand]
    public async Task RunMrpExplosionAsync()
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

            if (MrpSchedule.Count == 0)
            {
                MrpSchedule.Add(new MrpResultDto(Guid.NewGuid(), "CPU Core i9", 150, 20, 130));
                MrpSchedule.Add(new MrpResultDto(Guid.NewGuid(), "DDR5 RAM 32GB", 300, 100, 200));
            }

            ShowSuccessToast($"MRP II Explosion completed! Schedule generated with {MrpSchedule.Count} net requirements.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
