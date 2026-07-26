using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Inventory.Events;

public record ProductPriceUpdatedEvent(ProductId ProductId, decimal NewStandardCost, decimal NewListPrice) : IDomainEvent;
