using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Financials.Commands.PostJournalEntry;
using YAERP.Application.Sales.Commands.ProcessPosSale;
using YAERP.Domain.Identity;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Application.Tests.Financials;

public class FinancialsCommandTests
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
    public async Task PostJournalEntry_ShouldFail_WhenDebitsAndCreditsAreUnbalanced()
    {
        using var db = CreateDbContext(out var tenantContext);
        var handler = new PostJournalEntryCommandHandler(db, tenantContext);

        var lines = new List<JournalLineDto>
        {
            new JournalLineDto("1010", "Cash", "Debit", 5000m, 0m),
            new JournalLineDto("4010", "Revenue", "Credit", 0m, 4500m) // Unbalanced!
        };

        var command = new PostJournalEntryCommand("JV-001", "Unbalanced entry", lines);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Journal.Unbalanced", result.Error.Code);
    }

    [Fact]
    public async Task PostJournalEntry_ShouldSucceed_WhenDebitsEqualsCredits()
    {
        using var db = CreateDbContext(out var tenantContext);
        var handler = new PostJournalEntryCommandHandler(db, tenantContext);

        var lines = new List<JournalLineDto>
        {
            new JournalLineDto("1010", "Cash", "Debit", 5000m, 0m),
            new JournalLineDto("4010", "Revenue", "Credit", 0m, 5000m) // Balanced!
        };

        var command = new PostJournalEntryCommand("JV-002", "Balanced entry", lines);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task ProcessPosSale_ShouldCreateFulfilledOrder()
    {
        using var db = CreateDbContext(out var tenantContext);
        var handler = new ProcessPosSaleCommandHandler(db, tenantContext);

        var items = new List<PosCartLineDto>
        {
            new PosCartLineDto(Guid.NewGuid(), "SKU-01", "Wireless Mouse", 2m, 25m)
        };

        var command = new ProcessPosSaleCommand("POS-9001", 50m, 5m, 55m, "Cash", 100m, 45m, items);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Equal(1, await db.SalesOrders.CountAsync());
    }
}
