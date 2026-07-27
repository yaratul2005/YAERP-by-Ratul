using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Infrastructure.Warehouse;

namespace YAERP.Infrastructure.Tests.Warehouse;

public class SlottingOptimizationServiceTests : IDisposable
{
    private readonly TestWmsDbContext _dbContext;
    private readonly SlottingOptimizationService _sut;

    public SlottingOptimizationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestWmsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestWmsDbContext(options);
        _sut = new SlottingOptimizationService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task RecommendBinsAsync_ShouldReturnCompatibleBins()
    {
        var warehouseId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var binId = Guid.NewGuid();

        _dbContext.WarehouseZones.Add(new WarehouseZone { Id = zoneId, WarehouseId = warehouseId, ZoneType = "Picking" });
        _dbContext.WarehouseBins.Add(new WarehouseBin { Id = binId, ZoneId = zoneId, BinCode = "TEST-BIN", MaxVolumeCubicMeters = 100m, MaxWeightKg = 1000m });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.RecommendBinsAsync(warehouseId, 50m, 500m, false, "A");

        Assert.Single(result);
        Assert.Equal(binId, result[0].BinId);
        Assert.Equal(100m, result[0].AvailableVolume);
    }

    [Fact]
    public async Task RecommendBinsAsync_ColdStorageConstraint_FiltersCorrectly()
    {
        var warehouseId = Guid.NewGuid();
        var coldZoneId = Guid.NewGuid();
        var normalZoneId = Guid.NewGuid();

        _dbContext.WarehouseZones.Add(new WarehouseZone { Id = coldZoneId, WarehouseId = warehouseId, ZoneType = "ColdStorage" });
        _dbContext.WarehouseZones.Add(new WarehouseZone { Id = normalZoneId, WarehouseId = warehouseId, ZoneType = "Picking" });

        _dbContext.WarehouseBins.Add(new WarehouseBin { Id = Guid.NewGuid(), ZoneId = coldZoneId, BinCode = "COLD-BIN", MaxVolumeCubicMeters = 100m, MaxWeightKg = 1000m, IsTemperatureControlled = true });
        _dbContext.WarehouseBins.Add(new WarehouseBin { Id = Guid.NewGuid(), ZoneId = normalZoneId, BinCode = "NORMAL-BIN", MaxVolumeCubicMeters = 100m, MaxWeightKg = 1000m, IsTemperatureControlled = false });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.RecommendBinsAsync(warehouseId, 50m, 500m, true, "B");

        Assert.Single(result);
        Assert.Equal("COLD-BIN", result[0].BinCode);
    }
}
