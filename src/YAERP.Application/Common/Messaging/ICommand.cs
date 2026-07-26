using MediatR;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Common.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
