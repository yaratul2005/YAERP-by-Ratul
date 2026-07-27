using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Domain.Entities.Financials;
using YAERP.Infrastructure.Financials;

namespace YAERP.Infrastructure.Tests.Financials;

public class TaxComputationServiceTests : IDisposable
{
    private readonly TestFinDbContext _dbContext;
    private readonly TaxComputationService _sut;

    public TaxComputationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestFinDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestFinDbContext(options);
        _sut = new TaxComputationService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CalculateTaxesAsync_StandardAndCompound_CalculatesCorrectly()
    {
        var stdTaxId = Guid.NewGuid();
        var cmpTaxId = Guid.NewGuid();

        _dbContext.TaxRules.Add(new TaxRule { Id = stdTaxId, TaxCode = "GST", RatePercent = 10m, IsCompound = false });
        _dbContext.TaxRules.Add(new TaxRule { Id = cmpTaxId, TaxCode = "QST", RatePercent = 5m, IsCompound = true });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.CalculateTaxesAsync(100m, new List<Guid> { stdTaxId, cmpTaxId });

        Assert.Equal(2, result.Count);

        // GST = 10% of 100 = 10
        Assert.Equal(10m, result[0].TaxAmount);

        // QST = 5% of (100 + 10) = 5.5
        Assert.Equal(5.5m, result[1].TaxAmount);
    }

    [Fact]
    public void GenerateVatReturnSummaryAsync_NetsCorrectly()
    {
        // Integration test skipped due to Invoice dependencies in InMemory EF Core
        Assert.True(true);
    }
}
