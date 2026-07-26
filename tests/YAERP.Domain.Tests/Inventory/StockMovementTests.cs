using System;
using System.Linq;
using Xunit;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Inventory.Events;

namespace YAERP.Domain.Tests.Inventory;

public class StockMovementTests
{
    [Fact]
    public void Create_Should_SetPropertiesAndRaiseEvent()
    {
        // Arrange
        var tenantId = new TenantId(Guid.NewGuid());
        var productId = new ProductId(Guid.NewGuid());
        var warehouseId = new WarehouseId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var type = StockMovementType.InboundReceipt;

        // Act
        var movement = StockMovement.Create(tenantId, productId, warehouseId, type, 100m, 15m, "REF123", userId);

        // Assert
        Assert.Equal(tenantId, movement.TenantId);
        Assert.Equal(productId, movement.ProductId);
        Assert.Equal(warehouseId, movement.WarehouseId);
        Assert.Equal(type, movement.MovementType);
        Assert.Equal(100m, movement.Quantity);
        Assert.Equal(15m, movement.UnitCost);
        Assert.Equal("REF123", movement.ReferenceNumber);
        Assert.Equal(userId, movement.CreatedByUserId);

        var recordedEvent = movement.DomainEvents.OfType<StockMovementRecordedEvent>().SingleOrDefault();
        Assert.NotNull(recordedEvent);
        Assert.Equal(movement.Id, recordedEvent.StockMovementId);
        Assert.Equal(productId, recordedEvent.ProductId);
        Assert.Equal(warehouseId, recordedEvent.WarehouseId);
    }
}
