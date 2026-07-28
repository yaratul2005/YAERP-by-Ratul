using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Manufacturing.Commands.RecordProductionOutput;

namespace YAERP.UI.ViewModels.Modals;

public partial class ShopFloorOutputModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private Guid _workOrderId;

    [ObservableProperty]
    private string _workOrderNumber = "WO-1002";

    [ObservableProperty]
    private decimal _completedQuantity = 10m;

    [ObservableProperty]
    private decimal _scrappedQuantity = 1m;

    [ObservableProperty]
    private string _scrapReason = "Tolerance out-of-spec during milling";

    [ObservableProperty]
    private decimal _laborHours = 3.5m;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ShopFloorOutputModalViewModel(
        IMediator mediator,
        Guid workOrderId = default,
        string workOrderNumber = "WO-1002",
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _workOrderId = workOrderId != default ? workOrderId : Guid.NewGuid();
        _workOrderNumber = workOrderNumber;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;
    }

    [RelayCommand]
    public async Task SubmitProductionOutputAsync()
    {
        if (CompletedQuantity <= 0 && ScrappedQuantity <= 0)
        {
            StatusMessage = "Completed quantity or scrapped quantity must be greater than zero.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Recording shop floor output & updating WIP inventory...";

        try
        {
            var command = new RecordProductionOutputCommand(
                WorkOrderId,
                CompletedQuantity,
                ScrappedQuantity,
                ScrapReason,
                LaborHours);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Recorded {CompletedQuantity} completed units for {WorkOrderNumber}!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Submission failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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
