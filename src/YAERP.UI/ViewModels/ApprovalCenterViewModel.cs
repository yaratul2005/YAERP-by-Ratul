using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Approvals.Commands.ProcessApprovalDecision;
using YAERP.Application.Common.Interfaces;

namespace YAERP.UI.ViewModels;

public partial class ApprovalCenterViewModel : ObservableObject
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
        _mediator = mediator;
        _context = context;

        _ = LoadPendingRequestsAsync();
    }

    [RelayCommand]
    private async Task LoadPendingRequestsAsync()
    {
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

            TotalPendingCount = PendingRequests.Count;
            SelectedRequest = PendingRequests.FirstOrDefault();
            StatusMessage = TotalPendingCount > 0
                ? $"Loaded {TotalPendingCount} pending approval request(s)."
                : "No pending approval requests requiring authorization.";
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
    private async Task ApproveAsync()
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
                StatusMessage = $"✅ Successfully approved {SelectedRequest.EntityType} {SelectedRequest.EntityId}.";
                DecisionComments = string.Empty;
                await LoadPendingRequestsAsync();
            }
            else
            {
                StatusMessage = $"❌ Approval failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during approval: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task RejectAsync()
    {
        if (SelectedRequest == null || IsProcessing) return;

        IsProcessing = true;
        StatusMessage = $"Rejecting transaction {SelectedRequest.EntityId}...";

        try
        {
            var command = new ProcessApprovalDecisionCommand(
                SelectedRequest.Id,
                "Current_Operator",
                IsApproved: false,
                Comments: string.IsNullOrWhiteSpace(DecisionComments) ? "Rejected via Approval Center UI." : DecisionComments);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                StatusMessage = $"🚫 Transaction {SelectedRequest.EntityId} rejected.";
                DecisionComments = string.Empty;
                await LoadPendingRequestsAsync();
            }
            else
            {
                StatusMessage = $"❌ Rejection failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during rejection: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
