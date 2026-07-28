using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Sales.Commands.UpdateDealStage;

public class UpdateDealStageCommandHandler : ICommandHandler<UpdateDealStageCommand, bool>
{
    public Task<Result<bool>> Handle(UpdateDealStageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewStage))
        {
            return Task.FromResult(Result.Failure<bool>(new Error("Deal.InvalidStage", "Pipeline stage cannot be empty.", ErrorType.Validation)));
        }

        return Task.FromResult(Result.Success(true));
    }
}
