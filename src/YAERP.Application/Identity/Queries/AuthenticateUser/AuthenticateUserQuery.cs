using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Identity.Queries.AuthenticateUser;

public record AuthenticateUserQuery(
    Guid TenantId,
    string Email,
    string Password) : IQuery<AuthenticationResult>;

public record AuthenticationResult(
    Guid UserId,
    Guid TenantId,
    string Email,
    string Token);
