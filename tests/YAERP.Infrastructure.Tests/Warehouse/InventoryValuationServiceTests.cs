using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Infrastructure.Warehouse;

namespace YAERP.Infrastructure.Tests.Warehouse;

public class InventoryValuationServiceTests : IDisposable
{
    private readonly TestWmsDbContext _dbContext;
    private readonly InventoryValuationService _sut;

    public InventoryValuationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestWmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestWmsDbContext(options);
        _sut = new InventoryValuationService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ConsumeInventoryAsync_FIFO_ShouldConsumeOldestLayersFirst()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-10), RemainingQuantity = 100, UnitCost = 10m });
        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-5), RemainingQuantity = 100, UnitCost = 12m });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ConsumeInventoryAsync(productId, warehouseId, 150, ValuationMethod.FIFO);

        // Should take 100 @ 10m + 50 @ 12m = 1000 + 600 = 1600 total cost
        Assert.Equal(150m, result.QuantityConsumed);
        Assert.Equal(1600m, result.TotalCost);
    }

    [Fact]
    public async Task ConsumeInventoryAsync_LIFO_ShouldConsumeNewestLayersFirst()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-10), RemainingQuantity = 100, UnitCost = 10m });
        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-5), RemainingQuantity = 100, UnitCost = 12m });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ConsumeInventoryAsync(productId, warehouseId, 150, ValuationMethod.LIFO);

        // Should take 100 @ 12m + 50 @ 10m = 1200 + 500 = 1700 total cost
        Assert.Equal(150m, result.QuantityConsumed);
        Assert.Equal(1700m, result.TotalCost);
    }

    [Fact]
    public async Task ConsumeInventoryAsync_AVCO_ShouldCalculateWeightedAverage()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-10), RemainingQuantity = 100, UnitCost = 10m });
        _dbContext.InventoryCostLayers.Add(new InventoryCostLayer { Id = Guid.NewGuid(), ProductId = productId, WarehouseId = warehouseId, LayerDateUtc = DateTime.UtcNow.AddDays(-5), RemainingQuantity = 100, UnitCost = 15m });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ConsumeInventoryAsync(productId, warehouseId, 100, ValuationMethod.AVCO);

        // Total stock = 200, Total value = 1000 + 1500 = 2500, AVCO = 12.5m
        Assert.Equal(100m, result.QuantityConsumed);
        Assert.Equal(12.5m, result.UnitCost);
        Assert.Equal(1250m, result.TotalCost);
    }

    [Fact]
    public async Task ConsumeInventoryAsync_StandardCost_ShouldReturnStandardCost()
    {
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        var result = await _sut.ConsumeInventoryAsync(productId, warehouseId, 50, ValuationMethod.StandardCost, 8m);

        Assert.Equal(50m, result.QuantityConsumed);
        Assert.Equal(8m, result.UnitCost);
        Assert.Equal(400m, result.TotalCost);
    }
}
