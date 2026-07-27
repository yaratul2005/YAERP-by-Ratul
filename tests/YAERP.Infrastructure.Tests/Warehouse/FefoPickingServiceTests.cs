using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Infrastructure.Warehouse;

namespace YAERP.Infrastructure.Tests.Warehouse;

public class FefoPickingServiceTests : IDisposable
{
    private readonly TestWmsDbContext _dbContext;
    private readonly FefoPickingService _sut;

    public FefoPickingServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestWmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestWmsDbContext(options);
        _sut = new FefoPickingService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GenerateFefoPickRouteAsync_ShouldPrioritizeEarliestExpiration()
    {
        var productId = Guid.NewGuid();

        var lot1 = new InventoryLot { Id = Guid.NewGuid(), ProductId = productId, LotNumber = "L1", ExpirationDate = DateTime.UtcNow.AddDays(10) };
        var lot2 = new InventoryLot { Id = Guid.NewGuid(), ProductId = productId, LotNumber = "L2", ExpirationDate = DateTime.UtcNow.AddDays(2) }; // Should be picked first

        _dbContext.InventoryLots.Add(lot1);
        _dbContext.InventoryLots.Add(lot2);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GenerateFefoPickRouteAsync(productId, 150m);

        Assert.Equal(2, result.Count);
        // We know qtyAvailableInLot is mocked to 100m, so it should take 100 from L2, then 50 from L1
        // (the mock FefoPickingService orders by bin code at the end so we check if lot2 is included with 100)

        var firstLotPicked = result.FirstOrDefault(x => x.LotId == lot2.Id);
        Assert.NotNull(firstLotPicked);
        Assert.Equal(100m, firstLotPicked.QuantityToPick);

        var secondLotPicked = result.FirstOrDefault(x => x.LotId == lot1.Id);
        Assert.NotNull(secondLotPicked);
        Assert.Equal(50m, secondLotPicked.QuantityToPick);
    }

    [Fact]
    public async Task ForwardTraceAsync_ShouldReturnGenealogyNodes()
    {
        var lot = new InventoryLot { Id = Guid.NewGuid(), SupplierLotRef = "SUP-123", LotNumber = "INT-001" };
        _dbContext.InventoryLots.Add(lot);
        await _dbContext.SaveChangesAsync();

        var trace = await _sut.ForwardTraceAsync("SUP-123");

        Assert.NotEmpty(trace);
        Assert.Contains(trace, x => x.NodeType == "PurchaseOrder");
    }
}
