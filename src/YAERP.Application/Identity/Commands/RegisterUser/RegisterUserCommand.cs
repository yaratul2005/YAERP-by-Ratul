using YAERP.Application.Common.Messaging;
using YAERP.Domain.Identity;

namespace YAERP.Application.Identity.Commands.RegisterUser;

public record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Guid>;
