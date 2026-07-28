using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Manufacturing.Commands.RecordProductionOutput;

public class RecordProductionOutputCommandHandler : ICommandHandler<RecordProductionOutputCommand, bool>
{
    public Task<Result<bool>> Handle(RecordProductionOutputCommand request, CancellationToken cancellationToken)
    {
        if (request.CompletedQuantity <= 0 && request.ScrappedQuantity <= 0)
        {
            return Task.FromResult(Result.Failure<bool>(new Error("WorkOrder.InvalidOutput", "Output completed quantity or scrapped quantity must be greater than zero.", ErrorType.Validation)));
        }

        return Task.FromResult(Result.Success(true));
    }
}
