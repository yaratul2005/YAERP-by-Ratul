using System;
using System.Collections.Generic;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities;

public class ApprovalRequest : Entity<Guid>
{
    private readonly List<ApprovalStepLog> _stepLogs = new();

    private ApprovalRequest(
        Guid id,
        string entityType,
        string entityId,
        decimal transactionAmount,
        string requestedByUserId,
        string initialApprovalLevel,
        DateTime createdAtUtc) : base(id)
    {
        EntityType = entityType;
        EntityId = entityId;
        TransactionAmount = transactionAmount;
        RequestedByUserId = requestedByUserId;
        CurrentApprovalLevel = initialApprovalLevel;
        Status = "Pending";
        CreatedAtUtc = createdAtUtc;
    }

    private ApprovalRequest() { }

    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public decimal TransactionAmount { get; private set; }
    public string RequestedByUserId { get; private set; } = string.Empty;
    public string CurrentApprovalLevel { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<ApprovalStepLog> StepLogs => _stepLogs;

    public static ApprovalRequest Create(
        string entityType,
        string entityId,
        decimal transactionAmount,
        string requestedByUserId,
        string initialApprovalLevel)
    {
        return new ApprovalRequest(
            Guid.NewGuid(),
            entityType,
            entityId,
            transactionAmount,
            requestedByUserId,
            initialApprovalLevel,
            DateTime.UtcNow);
    }

    public ApprovalStepLog ApproveStep(string processedByUserId, string? nextApprovalLevel, string? comments)
    {
        if (Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot approve request in '{Status}' state.");
        }

        var log = ApprovalStepLog.Create(Id, CurrentApprovalLevel, "Approved", processedByUserId, comments);
        _stepLogs.Add(log);

        if (!string.IsNullOrWhiteSpace(nextApprovalLevel))
        {
            CurrentApprovalLevel = nextApprovalLevel;
        }
        else
        {
            Status = "Approved";
            CompletedAtUtc = DateTime.UtcNow;
        }

        return log;
    }

    public ApprovalStepLog Reject(string processedByUserId, string? comments)
    {
        if (Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot reject request in '{Status}' state.");
        }

        var log = ApprovalStepLog.Create(Id, CurrentApprovalLevel, "Rejected", processedByUserId, comments);
        _stepLogs.Add(log);

        Status = "Rejected";
        RejectionReason = comments ?? "Request rejected by approver.";
        CompletedAtUtc = DateTime.UtcNow;

        return log;
    }
}
