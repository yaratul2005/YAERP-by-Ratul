using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Application.Identity.Commands.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = new TenantId(request.TenantId);

        // Normally we wouldn't check if tenant exists directly unless necessary, but let's assume valid.
        var emailExists = await _context.Users
            .IgnoreQueryFilters() // Must ignore to check across or within specifically
            .AnyAsync(u => u.TenantId == tenantId && u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(new Error("User.DuplicateEmail", "Email already in use.", ErrorType.Conflict));
        }

        // Mock hashing for now, IRL we'd use IPasswordHasher
        var passwordHash = $"HASHED_{request.Password}";

        var user = User.Create(
            tenantId,
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id.Value);
    }
}
