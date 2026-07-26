using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities;

namespace YAERP.Infrastructure.Workflows;

public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ApprovalWorkflowService> _logger;

    public ApprovalWorkflowService(
        IApplicationDbContext context,
        ILogger<ApprovalWorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApprovalRequestDto?> InitiateApprovalAsync(
        string entityType,
        string entityId,
        decimal amount,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Amount < $5,000 -> Auto-Approved, no approval request needed
        if (amount < 5000m)
        {
            _logger.LogInformation("Transaction {EntityType}:{EntityId} for ${Amount:F2} auto-approved (Threshold < $5,000)", entityType, entityId, amount);
            return null;
        }

        // 2. Determine initial level
        string initialLevel = "Tier1_Supervisor";

        var request = ApprovalRequest.Create(
            entityType: entityType,
            entityId: entityId,
            transactionAmount: amount,
            requestedByUserId: userId,
            initialApprovalLevel: initialLevel);

        _context.ApprovalRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initiated multi-level approval request {RequestId} for {EntityType}:{EntityId} (${Amount:F2}) at level {Level}",
            request.Id, entityType, entityId, amount, initialLevel);

        return new ApprovalRequestDto(
            request.Id,
            request.EntityType,
            request.EntityId,
            request.TransactionAmount,
            request.RequestedByUserId,
            request.CurrentApprovalLevel,
            request.Status,
            request.RejectionReason,
            request.CreatedAtUtc,
            request.CompletedAtUtc);
    }

    public async Task<bool> ProcessApprovalStepAsync(
        Guid approvalRequestId,
        string userId,
        bool isApproved,
        string? comments,
        CancellationToken cancellationToken = default)
    {
        var request = await _context.ApprovalRequests
            .Include(r => r.StepLogs)
            .FirstOrDefaultAsync(r => r.Id == approvalRequestId, cancellationToken);

        if (request == null || request.Status != "Pending")
        {
            _logger.LogWarning("Approval request {RequestId} not found or not in Pending state.", approvalRequestId);
            return false;
        }

        if (isApproved)
        {
            // Tier 1 -> Tier 2 escalation if amount >= $25,000
            if (request.CurrentApprovalLevel == "Tier1_Supervisor" && request.TransactionAmount >= 25000m)
            {
                var log = request.ApproveStep(userId, "Tier2_FinanceManager", comments);
                _context.ApprovalStepLogs.Add(log);
                _logger.LogInformation("Approval request {RequestId} Tier 1 approved by {UserId}. Escalating to Tier2_FinanceManager.", approvalRequestId, userId);
            }
            else
            {
                // Final approval
                var log = request.ApproveStep(userId, null, comments);
                _context.ApprovalStepLogs.Add(log);
                _logger.LogInformation("Approval request {RequestId} fully approved by {UserId}.", approvalRequestId, userId);
            }
        }
        else
        {
            var log = request.Reject(userId, comments);
            _context.ApprovalStepLogs.Add(log);
            _logger.LogInformation("Approval request {RequestId} rejected by {UserId}. Reason: {Comments}", approvalRequestId, userId, comments);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
