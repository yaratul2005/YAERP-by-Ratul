using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Warehouse.Commands.ReceiveInventoryShipment;
using YAERP.Application.Warehouse.Commands.TransferStockBetweenBins;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Domain.Identity;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Application.Tests.Warehouse;

public class WarehouseCommandTests
{
    private class TestTenantContext : ITenantContext
    {
        public TenantId? CurrentTenantId { get; } = new TenantId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        public UserId? CurrentUserId { get; } = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    }

    private YAERPDbContext CreateDbContext(out TestTenantContext tenantContext)
    {
        var dbOptions = new DbContextOptionsBuilder<YAERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        tenantContext = new TestTenantContext();
        var auditInterceptor = new AuditSaveInterceptor(tenantContext);
        var syncInterceptor = new SyncOutboxInterceptor();

        return new YAERPDbContext(dbOptions, tenantContext, auditInterceptor, syncInterceptor);
    }

    [Fact]
    public async Task TransferStock_ShouldFail_WhenBinsAreIdentical()
    {
        using var db = CreateDbContext(out _);
        var handler = new TransferStockBetweenBinsCommandHandler(db);
        var binId = Guid.NewGuid();

        var command = new TransferStockBetweenBinsCommand(binId, binId, Guid.NewGuid(), 10m, "Test relocation");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Transfer.InvalidBins", result.Error.Code);
    }

    [Fact]
    public async Task TransferStock_ShouldSucceed_WhenBinsAreValidAndUnlocked()
    {
        using var db = CreateDbContext(out _);
        var sourceBin = new WarehouseBin { Id = Guid.NewGuid(), BinCode = "BIN-SRC-01", IsLocked = false };
        var destBin = new WarehouseBin { Id = Guid.NewGuid(), BinCode = "BIN-DST-02", IsLocked = false };

        db.WarehouseBins.AddRange(sourceBin, destBin);
        await db.SaveChangesAsync();

        var handler = new TransferStockBetweenBinsCommandHandler(db);
        var command = new TransferStockBetweenBinsCommand(sourceBin.Id, destBin.Id, Guid.NewGuid(), 15m, "Relocate stock");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ReceiveShipment_ShouldCreateCostLayerAndStockMovement()
    {
        using var db = CreateDbContext(out var tenantContext);
        var mockSlotting = new Mock<ISlottingOptimizationService>();
        mockSlotting.Setup(s => s.RecommendBinsAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new System.Collections.Generic.List<SlottingRecommendation>
            {
                new SlottingRecommendation(Guid.NewGuid(), "BIN-REC-01", 10.0m, 500m)
            });

        var handler = new ReceiveInventoryShipmentCommandHandler(db, mockSlotting.Object, tenantContext);
        var command = new ReceiveInventoryShipmentCommand(
            "PO-10099", Guid.NewGuid(), "Organic Apples", 100m, 2.50m, 0.5m, 100m, false, "A");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Equal(1, await db.InventoryCostLayers.CountAsync());
        Assert.Equal(1, await db.StockMovements.CountAsync());
    }
}
