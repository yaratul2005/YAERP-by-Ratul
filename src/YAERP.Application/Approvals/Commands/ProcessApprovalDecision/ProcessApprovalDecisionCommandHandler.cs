using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Approvals.Commands.ProcessApprovalDecision;

public sealed class ProcessApprovalDecisionCommandHandler
    : ICommandHandler<ProcessApprovalDecisionCommand, bool>
{
    private readonly IApprovalWorkflowService _workflowService;

    public ProcessApprovalDecisionCommandHandler(IApprovalWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public async Task<Result<bool>> Handle(
        ProcessApprovalDecisionCommand request,
        CancellationToken cancellationToken)
    {
        bool success = await _workflowService.ProcessApprovalStepAsync(
            request.ApprovalRequestId,
            request.UserId,
            request.IsApproved,
            request.Comments,
            cancellationToken);

        if (!success)
        {
            return Result.Failure<bool>(new Error(
                "Approval.ProcessFailed",
                "Failed to process approval step or request was not found.",
                ErrorType.Validation));
        }

        return Result.Success(true);
    }
}
