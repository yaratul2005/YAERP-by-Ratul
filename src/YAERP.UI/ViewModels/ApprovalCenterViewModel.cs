using System;
using YAERP.UI.Workspace;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Approvals.Commands.ProcessApprovalDecision;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Drawers;
using YAERP.UI.Views.Modals;

namespace YAERP.UI.ViewModels;

public partial class ApprovalCenterViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    [ObservableProperty]
    private ObservableCollection<ApprovalRequestDto> _pendingRequests = new();

    [ObservableProperty]
    private ApprovalRequestDto? _selectedRequest;

    [ObservableProperty]
    private string _decisionComments = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private int _totalPendingCount;

    [ObservableProperty]
    private string? _statusMessage;

    public ApprovalCenterViewModel(IMediator mediator, IApplicationDbContext context)
    {
        Title = "Approval Center";
        IconKey = "🛡️";
        TabId = "ApprovalCenterViewModel";
        _mediator = mediator;
        _context = context;
    }

    public override async Task OnTabActivatedAsync()
    {
        await LoadPendingRequestsAsync();
    }

    [RelayCommand]
    public async Task LoadPendingRequestsAsync()
    {
        if (IsProcessing) return;
        IsProcessing = true;
        StatusMessage = "Loading pending approval requests...";

        try
        {
            var requests = await _context.ApprovalRequests
                .AsNoTracking()
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            PendingRequests.Clear();
            foreach (var req in requests)
            {
                PendingRequests.Add(new ApprovalRequestDto(
                    req.Id,
                    req.EntityType,
                    req.EntityId,
                    req.TransactionAmount,
                    req.RequestedByUserId,
                    req.CurrentApprovalLevel,
                    req.Status,
                    req.RejectionReason,
                    req.CreatedAtUtc,
                    req.CompletedAtUtc));
            }

            if (PendingRequests.Count == 0)
            {
                // Add sample fallback items for UI demonstration if DB empty
                PendingRequests.Add(new ApprovalRequestDto(Guid.NewGuid(), "PurchaseOrder", "PO-991", 45000.00m, "John (Procurement)", "Tier2_FinanceManager", "Pending", null, DateTime.UtcNow.AddHours(-2), null));
                PendingRequests.Add(new ApprovalRequestDto(Guid.NewGuid(), "SalesDiscount", "SO-108", 12500.00m, "Alice (Sales Rep)", "Tier1_Supervisor", "Pending", null, DateTime.UtcNow.AddHours(-5), null));
            }

            TotalPendingCount = PendingRequests.Count;
            SelectedRequest = PendingRequests.FirstOrDefault();
            StatusMessage = $"Loaded {TotalPendingCount} pending authorization request(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading approval requests: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    public void OpenApprovalInspectorDrawer(ApprovalRequestDto? request)
    {
        var req = request ?? SelectedRequest;
        if (req == null) return;

        var drawerVm = new ApprovalInspectorDrawerViewModel(
            _mediator,
            req.Id,
            req.EntityType,
            req.EntityId,
            req.TransactionAmount,
            req.RequestedByUserId,
            req.CurrentApprovalLevel,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = LoadPendingRequestsAsync();
            },
            onCloseRequested: () => CloseDrawer());

        var view = new ApprovalInspectorDrawer { DataContext = drawerVm };
        OpenDrawer(view, $"Approval Inspector - #{req.EntityId}");
    }

    [RelayCommand]
    public void OpenRejectionReasonModal()
    {
        if (SelectedRequest == null)
        {
            ShowErrorToast("Please select a pending request to reject.");
            return;
        }

        var modalVm = new RejectionReasonModalViewModel(
            _mediator,
            SelectedRequest.Id,
            SelectedRequest.EntityId,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = LoadPendingRequestsAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new RejectionReasonModal { DataContext = modalVm };
        OpenModal(view, $"Reject Request #{SelectedRequest.EntityId}");
    }

    [RelayCommand]
    public async Task ApproveSelectedAsync()
    {
        if (SelectedRequest == null || IsProcessing) return;

        IsProcessing = true;
        StatusMessage = $"Approving transaction {SelectedRequest.EntityId}...";

        try
        {
            var command = new ProcessApprovalDecisionCommand(
                SelectedRequest.Id,
                "Current_Operator",
                IsApproved: true,
                Comments: string.IsNullOrWhiteSpace(DecisionComments) ? "Approved via Approval Center UI." : DecisionComments);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                ShowSuccessToast($"Successfully approved {SelectedRequest.EntityType} #{SelectedRequest.EntityId}.");
                DecisionComments = string.Empty;
                await LoadPendingRequestsAsync();
            }
            else
            {
                ShowErrorToast($"Approval failed: {result.Error.Description}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorToast($"Error during approval: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
