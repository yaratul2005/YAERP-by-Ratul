using System;
using System.Collections.Generic;
using System.Linq;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Finance.Events;

namespace YAERP.Domain.Finance;

public class InvalidJournalEntryException : Exception
{
    public InvalidJournalEntryException(string message) : base(message) { }
}

public class JournalEntry : AggregateRoot<JournalEntryId>
{
    private readonly List<JournalLine> _lines = new();

    private JournalEntry(JournalEntryId id, TenantId tenantId, string entryNumber, DateTime postingDateUtc, string? description, bool isPosted) : base(id)
    {
        TenantId = tenantId;
        EntryNumber = entryNumber;
        PostingDateUtc = postingDateUtc;
        Description = description;
        IsPosted = isPosted;
    }

    private JournalEntry() { }

    public TenantId TenantId { get; private set; } = default!;
    public string EntryNumber { get; private set; } = string.Empty;
    public DateTime PostingDateUtc { get; private set; }
    public string? Description { get; private set; }
    public bool IsPosted { get; private set; }

    public IReadOnlyCollection<JournalLine> Lines => _lines.AsReadOnly();

    public static JournalEntry Create(TenantId tenantId, string entryNumber, string? description)
    {
        return new JournalEntry(new JournalEntryId(Guid.NewGuid()), tenantId, entryNumber, DateTime.UtcNow, description, false);
    }

    public void AddLine(AccountId accountId, decimal debit, decimal credit, string? memo)
    {
        if (IsPosted) throw new InvalidOperationException("Cannot modify a posted journal entry.");
        if (debit < 0 || credit < 0) throw new ArgumentException("Debit and credit must be non-negative.");
        if (debit > 0 && credit > 0) throw new ArgumentException("Line cannot have both debit and credit.");

        _lines.Add(new JournalLine(new JournalLineId(Guid.NewGuid()), accountId, debit, credit, memo));
    }

    public void Post()
    {
        if (IsPosted) throw new InvalidOperationException("Entry is already posted.");

        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
        {
            throw new InvalidJournalEntryException($"Journal entry is unbalanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}.");
        }

        IsPosted = true;
        AddDomainEvent(new JournalEntryPostedEvent(Id));
    }
}
