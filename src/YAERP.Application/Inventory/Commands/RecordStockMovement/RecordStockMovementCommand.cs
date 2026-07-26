using System;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Inventory.Commands.RecordStockMovement;

public record RecordStockMovementCommand(
    Guid ProductId,
    Guid WarehouseId,
    StockMovementType MovementType,
    decimal Quantity,
    decimal UnitCost,
    string? ReferenceNumber) : ICommand<Guid>;
