using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Approvals.Commands.ProcessApprovalDecision;

namespace YAERP.UI.ViewModels.Drawers;

public record TransactionLineItemDto(string LineDescription, decimal Quantity, decimal UnitPrice, decimal LineTotal);
public record ApprovalAuditStepDto(DateTime Timestamp, string TierLevel, string Action, string ProcessedBy, string Comments);

public partial class ApprovalInspectorDrawerViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private Guid _requestId;

    [ObservableProperty]
    private string _entityType = "PurchaseOrder";

    [ObservableProperty]
    private string _entityId = "PO-2023-994";

    [ObservableProperty]
    private decimal _transactionAmount = 14500.00m;

    [ObservableProperty]
    private string _requestedBy = "John Doe (Procurement Spec)";

    [ObservableProperty]
    private string _currentTier = "Tier2_FinanceManager";

    [ObservableProperty]
    private string _decisionComments = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<TransactionLineItemDto> LineItems { get; } = new();
    public ObservableCollection<ApprovalAuditStepDto> AuditSteps { get; } = new();

    public ApprovalInspectorDrawerViewModel(
        IMediator mediator,
        Guid requestId,
        string entityType,
        string entityId,
        decimal transactionAmount,
        string requestedBy,
        string currentTier,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _requestId = requestId;
        _entityType = entityType;
        _entityId = entityId;
        _transactionAmount = transactionAmount;
        _requestedBy = requestedBy;
        _currentTier = currentTier;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        LoadDetails();
    }

    private void LoadDetails()
    {
        LineItems.Clear();
        LineItems.Add(new TransactionLineItemDto("High-Performance Server Blade", 2m, 4500m, 9000m));
        LineItems.Add(new TransactionLineItemDto("Fiber Optic Switch (48-Port)", 1m, 5500m, 5500m));

        AuditSteps.Clear();
        AuditSteps.Add(new ApprovalAuditStepDto(DateTime.UtcNow.AddHours(-4), "Tier1_Supervisor", "Approved", "Sarah (Supervisor)", "Verified budget allocation. Escalated for Tier 2 sign-off."));
    }

    [RelayCommand]
    public async Task ApproveRequestAsync()
    {
        IsBusy = true;
        StatusMessage = "Processing tier approval decision...";

        try
        {
            var command = new ProcessApprovalDecisionCommand(
                RequestId,
                "Current_Operator",
                IsApproved: true,
                Comments: string.IsNullOrWhiteSpace(DecisionComments) ? "Approved via Approval Inspector Drawer." : DecisionComments);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Approved {EntityType} #{EntityId}!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Approval failed: {result.Error.Description}";
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
