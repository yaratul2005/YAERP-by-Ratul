using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Sales.Events;

public record SalesOrderFulfilledEvent(SalesOrderId SalesOrderId) : IDomainEvent;
