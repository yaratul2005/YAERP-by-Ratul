using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;
using YAERP.Domain.Identity;

namespace YAERP.Application.Financials.Commands.PostJournalEntry;

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
        if (request.Lines == null || request.Lines.Count == 0)
        {
            return Result.Failure<Guid>(new Error("Journal.NoLines", "Journal entry must contain at least one line item.", ErrorType.Validation));
        }

        decimal totalDebit = request.Lines.Sum(l => l.Debit);
        decimal totalCredit = request.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
        {
            return Result.Failure<Guid>(new Error("Journal.Unbalanced", $"Journal entry is unbalanced: Total Debit (${totalDebit:N2}) != Total Credit (${totalCredit:N2}).", ErrorType.Validation));
        }

        var tenantId = _tenantContext.CurrentTenantId ?? new TenantId(Guid.NewGuid());
        var journal = JournalEntry.Create(tenantId, request.ReferenceNumber, request.Description);

        foreach (var line in request.Lines)
        {
            journal.AddLine(new AccountId(Guid.NewGuid()), line.Debit, line.Credit, line.Description);
        }

        journal.Post();

        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(journal.Id.Value);
    }
}
