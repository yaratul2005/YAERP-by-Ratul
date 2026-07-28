using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Approvals.Commands.ProcessApprovalDecision;

namespace YAERP.UI.ViewModels.Modals;

public partial class RejectionReasonModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private Guid _requestId;

    [ObservableProperty]
    private string _entityId = string.Empty;

    [ObservableProperty]
    private string _rejectionReason = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public RejectionReasonModalViewModel(
        IMediator mediator,
        Guid requestId,
        string entityId,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _requestId = requestId;
        _entityId = entityId;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;
    }

    [RelayCommand]
    public async Task ConfirmRejectionAsync()
    {
        if (string.IsNullOrWhiteSpace(RejectionReason))
        {
            StatusMessage = "Rejection justification reason is mandatory.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Processing transaction rejection decision...";

        try
        {
            var command = new ProcessApprovalDecisionCommand(
                RequestId,
                "Current_Manager",
                IsApproved: false,
                Comments: RejectionReason);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Rejected transaction #{EntityId} with reason: '{RejectionReason}'");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Rejection failed: {result.Error.Description}";
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
