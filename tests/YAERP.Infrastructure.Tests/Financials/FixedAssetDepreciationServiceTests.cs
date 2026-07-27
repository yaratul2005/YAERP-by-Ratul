using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Domain.Entities.Financials;
using YAERP.Infrastructure.Financials;

namespace YAERP.Infrastructure.Tests.Financials;

public class FixedAssetDepreciationServiceTests : IDisposable
{
    private readonly TestFinDbContext _dbContext;
    private readonly FixedAssetDepreciationService _sut;

    public FixedAssetDepreciationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestFinDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestFinDbContext(options);
        _sut = new FixedAssetDepreciationService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task GenerateSchedule_StraightLine_CalculatesCorrectly()
    {
        var assetId = Guid.NewGuid();
        _dbContext.FixedAssets.Add(new FixedAsset
        {
            Id = assetId,
            AcquisitionCost = 10000m,
            SalvageValue = 1000m,
            UsefulLifeYears = 5,
            InServiceDate = new DateTime(2023, 1, 1),
            DepreciationMethod = "StraightLine"
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GenerateScheduleAsync(assetId);

        // 5 years * 12 months = 60 entries
        Assert.Equal(60, result.Count);

        // Yearly dep = (10000 - 1000) / 5 = 1800. Monthly = 150
        Assert.Equal(150m, result.First().DepreciationAmount);

        // Final book value should equal salvage value
        Assert.Equal(1000m, result.Last().BookValueAfter);
    }

    [Fact]
    public async Task GenerateSchedule_DoubleDeclining_CalculatesCorrectly()
    {
        var assetId = Guid.NewGuid();
        _dbContext.FixedAssets.Add(new FixedAsset
        {
            Id = assetId,
            AcquisitionCost = 10000m,
            SalvageValue = 1000m,
            UsefulLifeYears = 5,
            InServiceDate = new DateTime(2023, 1, 1),
            DepreciationMethod = "DoubleDeclining"
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GenerateScheduleAsync(assetId);

        // Year 1 DDB = (2/5) * 10000 = 4000. Monthly = 333.3333m
        // Note: Due to floating point division we should use precision checks
        Assert.Equal(60, result.Count);
        Assert.True(Math.Abs(333.3333m - result.First().DepreciationAmount) < 0.01m);

        // Final book value should equal salvage value
        Assert.Equal(1000m, result.Last().BookValueAfter);
    }

    [Fact]
    public async Task GenerateSchedule_SumOfYearsDigits_CalculatesCorrectly()
    {
        var assetId = Guid.NewGuid();
        _dbContext.FixedAssets.Add(new FixedAsset
        {
            Id = assetId,
            AcquisitionCost = 10000m,
            SalvageValue = 1000m,
            UsefulLifeYears = 5, // SYD denom = 15
            InServiceDate = new DateTime(2023, 1, 1),
            DepreciationMethod = "SumOfYearsDigits"
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.GenerateScheduleAsync(assetId);

        Assert.Equal(60, result.Count);

        // Year 1 SYD = 5/15 * 9000 = 3000. Monthly = 250
        Assert.True(Math.Abs(250m - result.First().DepreciationAmount) < 0.01m);

        // Final book value should equal salvage value
        Assert.Equal(1000m, result.Last().BookValueAfter);
    }
}
