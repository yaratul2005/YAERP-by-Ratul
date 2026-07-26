using MediatR;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
