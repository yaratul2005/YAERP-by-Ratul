using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Purchasing.Events;

public record PurchaseOrderReceivedEvent(PurchaseOrderId PurchaseOrderId) : IDomainEvent;
