using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory.Events;

namespace YAERP.Domain.Inventory;

public class StockMovement : AggregateRoot<StockMovementId>
{
    private StockMovement(
        StockMovementId id,
        TenantId tenantId,
        ProductId productId,
        WarehouseId warehouseId,
        StockMovementType movementType,
        decimal quantity,
        decimal unitCost,
        string? referenceNumber,
        DateTime movementDateUtc,
        UserId? createdByUserId) : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        WarehouseId = warehouseId;
        MovementType = movementType;
        Quantity = quantity;
        UnitCost = unitCost;
        ReferenceNumber = referenceNumber;
        MovementDateUtc = movementDateUtc;
        CreatedByUserId = createdByUserId;
    }

    private StockMovement() { } // EF Core

    public TenantId TenantId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public WarehouseId WarehouseId { get; private set; } = default!;
    public StockMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public DateTime MovementDateUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }

    public static StockMovement Create(
        TenantId tenantId,
        ProductId productId,
        WarehouseId warehouseId,
        StockMovementType movementType,
        decimal quantity,
        decimal unitCost,
        string? referenceNumber,
        UserId? createdByUserId)
    {
        var movement = new StockMovement(
            new StockMovementId(Guid.NewGuid()),
            tenantId,
            productId,
            warehouseId,
            movementType,
            quantity,
            unitCost,
            referenceNumber,
            DateTime.UtcNow,
            createdByUserId);

        movement.AddDomainEvent(new StockMovementRecordedEvent(movement.Id, movement.ProductId, movement.WarehouseId));

        return movement;
    }
}
