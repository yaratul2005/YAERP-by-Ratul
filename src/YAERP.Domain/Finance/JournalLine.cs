using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Finance;

public class JournalLine : Entity<JournalLineId>
{
    internal JournalLine(JournalLineId id, AccountId accountId, decimal debit, decimal credit, string? memo) : base(id)
    {
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Memo = memo;
    }

    private JournalLine() { }

    public JournalEntryId JournalEntryId { get; private set; } = default!;
    public AccountId AccountId { get; private set; } = default!;
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string? Memo { get; private set; }
}
