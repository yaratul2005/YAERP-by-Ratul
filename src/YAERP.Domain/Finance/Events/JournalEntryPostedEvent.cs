using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Finance.Events;

public record JournalEntryPostedEvent(JournalEntryId JournalEntryId) : IDomainEvent;
