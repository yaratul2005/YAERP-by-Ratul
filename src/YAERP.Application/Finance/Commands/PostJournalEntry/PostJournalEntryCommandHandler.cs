using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;

namespace YAERP.Application.Finance.Commands.PostJournalEntry;

public class PostJournalEntryCommandHandler : ICommandHandler<PostJournalEntryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public PostJournalEntryCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.JournalEntries.AnyAsync(j => j.EntryNumber == request.EntryNumber, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("JournalEntry.Duplicate", "Entry number must be unique.", ErrorType.Conflict));

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
            return Result.Failure<Guid>(new Error("JournalEntry.Unbalanced", "Debits must equal credits.", ErrorType.Validation));

        var je = JournalEntry.Create(
            tenantId,
            request.EntryNumber,
            request.Description);

        var accountIds = request.Lines.Select(l => new AccountId(l.AccountId)).ToList();
        var accounts = await _context.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        foreach (var line in request.Lines)
        {
            var accId = new AccountId(line.AccountId);
            if (!accounts.TryGetValue(accId, out var account))
            {
                return Result.Failure<Guid>(new Error("Account.NotFound", $"Account {line.AccountId} not found.", ErrorType.NotFound));
            }

            je.AddLine(accId, line.Debit, line.Credit, line.Memo);

            // Algebraic logic for balance depends on AccountType (Asset/Expense normal balance is Debit. Liability/Equity/Revenue is Credit).
            // This is a simplified application assuming debit adds, credit subtracts, but traditionally:
            decimal impact = 0;
            if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
            {
                impact = line.Debit - line.Credit;
            }
            else
            {
                impact = line.Credit - line.Debit;
            }

            account.UpdateBalance(impact);
        }

        je.Post(); // Checks balance and sets IsPosted = true

        _context.JournalEntries.Add(je);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(je.Id.Value);
    }
}
