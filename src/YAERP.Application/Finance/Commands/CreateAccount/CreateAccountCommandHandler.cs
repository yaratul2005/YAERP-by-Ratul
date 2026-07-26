using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;

namespace YAERP.Application.Finance.Commands.CreateAccount;

public class CreateAccountCommandHandler : ICommandHandler<CreateAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateAccountCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.Accounts.AnyAsync(a => a.AccountNumber == request.AccountNumber, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("Account.Duplicate", "Account number must be unique.", ErrorType.Conflict));

        var account = Account.Create(
            tenantId,
            request.AccountNumber,
            request.Name,
            request.Type);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(account.Id.Value);
    }
}
