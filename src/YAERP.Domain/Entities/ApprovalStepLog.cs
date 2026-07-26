using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Entities;

public class ApprovalStepLog : Entity<Guid>
{
    private ApprovalStepLog(
        Guid id,
        Guid approvalRequestId,
        string approvalLevel,
        string action,
        string processedByUserId,
        string? comments,
        DateTime timestampUtc) : base(id)
    {
        ApprovalRequestId = approvalRequestId;
        ApprovalLevel = approvalLevel;
        Action = action;
        ProcessedByUserId = processedByUserId;
        Comments = comments;
        TimestampUtc = timestampUtc;
    }

    private ApprovalStepLog() { }

    public Guid ApprovalRequestId { get; private set; }
    public string ApprovalLevel { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string ProcessedByUserId { get; private set; } = string.Empty;
    public string? Comments { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    public static ApprovalStepLog Create(
        Guid approvalRequestId,
        string approvalLevel,
        string action,
        string processedByUserId,
        string? comments)
    {
        return new ApprovalStepLog(
            Guid.NewGuid(),
            approvalRequestId,
            approvalLevel,
            action,
            processedByUserId,
            comments,
            DateTime.UtcNow);
    }
}
