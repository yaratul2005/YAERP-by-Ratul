using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Finance.Events;

public record InvoiceGeneratedEvent(InvoiceId InvoiceId) : IDomainEvent;
