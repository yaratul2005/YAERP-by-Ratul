using MediatR;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Common.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
