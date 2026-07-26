using System;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record ApprovalRequestDto(
    Guid Id,
    string EntityType,
    string EntityId,
    decimal TransactionAmount,
    string RequestedByUserId,
    string CurrentApprovalLevel,
    string Status,
    string? RejectionReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public interface IApprovalWorkflowService
{
    Task<ApprovalRequestDto?> InitiateApprovalAsync(
        string entityType,
        string entityId,
        decimal amount,
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> ProcessApprovalStepAsync(
        Guid approvalRequestId,
        string userId,
        bool isApproved,
        string? comments,
        CancellationToken cancellationToken = default);
}
