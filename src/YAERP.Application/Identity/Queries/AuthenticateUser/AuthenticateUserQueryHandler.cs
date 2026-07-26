using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Application.Identity.Queries.AuthenticateUser;

public class AuthenticateUserQueryHandler : IQueryHandler<AuthenticateUserQuery, AuthenticationResult>
{
    private readonly IApplicationDbContext _context;

    public AuthenticateUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AuthenticationResult>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);

        var user = await _context.Users
            .AsNoTracking() // Read paths bypass tracking overhead
            .IgnoreQueryFilters() // Just for auth as we might not be in tenant context yet
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<AuthenticationResult>(new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.NotFound));
        }

        // Mock verification
        var passwordHash = $"HASHED_{request.Password}";
        if (user.PasswordHash != passwordHash)
        {
            return Result.Failure<AuthenticationResult>(new Error("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.NotFound));
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationResult>(new Error("Auth.InactiveUser", "User account is inactive.", ErrorType.Failure));
        }

        // Generate fake token for now
        var token = "JWT_TOKEN_PLACEHOLDER";

        return Result.Success(new AuthenticationResult(
            user.Id.Value,
            user.TenantId.Value,
            user.Email,
            token));
    }
}
