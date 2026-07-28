using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Warehouse.Commands.TransferStockBetweenBins;

namespace YAERP.UI.ViewModels.Drawers;

public partial class BinTransferDrawerViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    public ObservableCollection<WarehouseBinDto> SourceBins { get; } = new();
    public ObservableCollection<WarehouseBinDto> DestinationBins { get; } = new();

    [ObservableProperty]
    private WarehouseBinDto? _selectedSourceBin;

    [ObservableProperty]
    private WarehouseBinDto? _selectedDestinationBin;

    [ObservableProperty]
    private string _selectedProductName = "Organic Milk (SKU-1001)";

    [ObservableProperty]
    private decimal _quantity = 10m;

    [ObservableProperty]
    private string _notes = "Routine rack relocation";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public BinTransferDrawerViewModel(
        IMediator mediator,
        ObservableCollection<WarehouseBinDto> availableBins,
        WarehouseBinDto? initialSourceBin = null,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        foreach (var bin in availableBins)
        {
            SourceBins.Add(bin);
            DestinationBins.Add(bin);
        }

        SelectedSourceBin = initialSourceBin ?? SourceBins.FirstOrDefault();
        SelectedDestinationBin = DestinationBins.FirstOrDefault(b => b.Id != SelectedSourceBin?.Id);
    }

    [RelayCommand]
    public async Task ExecuteTransferAsync()
    {
        if (SelectedSourceBin == null || SelectedDestinationBin == null)
        {
            StatusMessage = "Please select valid Source and Destination bins.";
            return;
        }

        if (SelectedSourceBin.Id == SelectedDestinationBin.Id)
        {
            StatusMessage = "Source and Destination bins cannot be identical.";
            return;
        }

        if (Quantity <= 0)
        {
            StatusMessage = "Transfer quantity must be greater than zero.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Executing stock relocation...";

        try
        {
            var command = new TransferStockBetweenBinsCommand(
                SelectedSourceBin.Id,
                SelectedDestinationBin.Id,
                Guid.NewGuid(), // Product ID
                Quantity,
                Notes);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Moved {Quantity} units from {SelectedSourceBin.BinCode} to {SelectedDestinationBin.BinCode}!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Transfer failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during transfer: {ex.Message}";
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
