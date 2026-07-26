using System;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Domain.Inventory.Events;

public record StockMovementRecordedEvent(StockMovementId StockMovementId, ProductId ProductId, WarehouseId WarehouseId) : IDomainEvent;
