using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Warehouse.Commands.ReceiveInventoryShipment;

namespace YAERP.UI.ViewModels.Modals;

public record SlottingRecommendationDto(Guid BinId, string BinCode, decimal AvailableVolume, decimal AvailableWeight);

public partial class GoodsReceivingModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly ISlottingOptimizationService _slottingService;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private string _supplierOrderRef = $"PO-REC-{DateTime.UtcNow:MMddHHmm}";

    [ObservableProperty]
    private string _productName = "Cold Pressed Olive Oil";

    [ObservableProperty]
    private decimal _quantity = 250m;

    [ObservableProperty]
    private decimal _unitCost = 18.50m;

    [ObservableProperty]
    private decimal _totalVolume = 1.2m;

    [ObservableProperty]
    private decimal _totalWeight = 225m;

    [ObservableProperty]
    private bool _requiresColdStorage;

    [ObservableProperty]
    private string _selectedAbcClass = "A";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<SlottingRecommendationDto> RecommendedBins { get; } = new();

    public GoodsReceivingModalViewModel(
        IMediator mediator,
        ISlottingOptimizationService slottingService,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _slottingService = slottingService;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;
    }

    [RelayCommand]
    public async Task CalculateSlottingAsync()
    {
        IsBusy = true;
        StatusMessage = "Calculating optimal warehouse bin slotting...";

        try
        {
            RecommendedBins.Clear();
            var warehouseId = Guid.NewGuid();
            var recommendations = await _slottingService.RecommendBinsAsync(
                warehouseId,
                TotalVolume,
                TotalWeight,
                RequiresColdStorage,
                SelectedAbcClass);

            foreach (var rec in recommendations)
            {
                RecommendedBins.Add(new SlottingRecommendationDto(rec.BinId, rec.BinCode, rec.AvailableVolume, rec.AvailableWeight));
            }

            if (RecommendedBins.Count == 0)
            {
                StatusMessage = "No matching unlocked bin found with required volume/weight capacity.";
            }
            else
            {
                StatusMessage = $"Found {RecommendedBins.Count} recommended destination bin(s).";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Slotting engine error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SubmitReceivingAsync()
    {
        if (string.IsNullOrWhiteSpace(SupplierOrderRef))
        {
            StatusMessage = "Supplier order reference is required.";
            return;
        }

        if (Quantity <= 0 || UnitCost <= 0)
        {
            StatusMessage = "Quantity and Unit Cost must be greater than zero.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Processing inventory receiving & cost layer creation...";

        try
        {
            var command = new ReceiveInventoryShipmentCommand(
                SupplierOrderRef,
                Guid.NewGuid(),
                ProductName,
                Quantity,
                UnitCost,
                TotalVolume,
                TotalWeight,
                RequiresColdStorage,
                SelectedAbcClass);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Received PO {SupplierOrderRef} ({Quantity}x {ProductName}) successfully!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Receiving failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during receiving: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCloseRequested?.Invoke();
    }
}
