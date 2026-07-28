using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Financials.Commands.PostDepreciationRun;

public class PostDepreciationRunCommandHandler : ICommandHandler<PostDepreciationRunCommand, bool>
{
    public Task<Result<bool>> Handle(PostDepreciationRunCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Task.FromResult(Result.Failure<bool>(new Error("Depreciation.InvalidAmount", "Depreciation amount must be greater than zero.", ErrorType.Validation)));
        }

        return Task.FromResult(Result.Success(true));
    }
}
