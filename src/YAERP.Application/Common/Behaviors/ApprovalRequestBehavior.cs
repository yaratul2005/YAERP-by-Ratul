using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Common.Behaviors;

public class ApprovalRequestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequireApproval, IRequest<TResponse>
{
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ILogger<ApprovalRequestBehavior<TRequest, TResponse>> _logger;

    public ApprovalRequestBehavior(
        IApprovalWorkflowService workflowService,
        ILogger<ApprovalRequestBehavior<TRequest, TResponse>> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.TotalAmount >= 5000m)
        {
            _logger.LogInformation("Intercepting transaction command {Command} for {EntityType}:{EntityId} (${Amount:F2}) requiring approval.",
                typeof(TRequest).Name, request.EntityType, request.EntityId, request.TotalAmount);

            var approvalDto = await _workflowService.InitiateApprovalAsync(
                request.EntityType,
                request.EntityId,
                request.TotalAmount,
                request.UserId,
                cancellationToken);

            if (approvalDto != null && approvalDto.Status == "Pending")
            {
                _logger.LogWarning("Transaction command {Command} intercepted! Multi-level approval created with ID {ApprovalId}.",
                    typeof(TRequest).Name, approvalDto.Id);

                var error = new Error(
                    "Approval.Required",
                    $"Transaction amount (${request.TotalAmount:F2}) exceeds $5,000 threshold and requires multi-level approval before execution. Approval Request ID: {approvalDto.Id}",
                    ErrorType.Validation);

                // Construct failed Result response reflectively if TResponse is Result or Result<T>
                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(error);
                }

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var genericType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result)
                        .GetMethod(nameof(Result.Failure))!
                        .MakeGenericMethod(genericType);

                    return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
                }
            }
        }

        return await next();
    }
}
