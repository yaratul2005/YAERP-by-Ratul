using System;
using System.Linq;
using Xunit;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Inventory.Events;

namespace YAERP.Domain.Tests.Inventory;

public class ProductTests
{
    [Fact]
    public void UpdatePrice_Should_UpdatePricesAndRaiseEvent()
    {
        // Arrange
        var tenantId = new TenantId(Guid.NewGuid());
        var unitOfMeasureId = new UnitOfMeasureId(Guid.NewGuid());
        var product = Product.Create(tenantId, "SKU1", "BC1", "Product1", null, null, unitOfMeasureId, 10m, 20m, false);

        // Act
        product.UpdatePrice(15m, 25m);

        // Assert
        Assert.Equal(15m, product.StandardCost);
        Assert.Equal(25m, product.ListPrice);

        var priceUpdatedEvent = product.DomainEvents.OfType<ProductPriceUpdatedEvent>().SingleOrDefault();
        Assert.NotNull(priceUpdatedEvent);
        Assert.Equal(15m, priceUpdatedEvent.NewStandardCost);
        Assert.Equal(25m, priceUpdatedEvent.NewListPrice);
    }

    [Fact]
    public void UpdatePrice_WithNegativeCost_Should_ThrowArgumentException()
    {
        // Arrange
        var tenantId = new TenantId(Guid.NewGuid());
        var unitOfMeasureId = new UnitOfMeasureId(Guid.NewGuid());
        var product = Product.Create(tenantId, "SKU1", "BC1", "Product1", null, null, unitOfMeasureId, 10m, 20m, false);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => product.UpdatePrice(-1m, 20m));
    }
}
