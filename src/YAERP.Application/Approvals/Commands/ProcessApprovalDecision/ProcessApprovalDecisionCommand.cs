using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Approvals.Commands.ProcessApprovalDecision;

public record ProcessApprovalDecisionCommand(
    Guid ApprovalRequestId,
    string UserId,
    bool IsApproved,
    string? Comments) : ICommand<bool>;
