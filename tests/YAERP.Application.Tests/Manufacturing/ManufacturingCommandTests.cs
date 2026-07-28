using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Manufacturing.Commands.CreateBomHeader;
using YAERP.Application.Manufacturing.Commands.RecordProductionOutput;
using YAERP.Domain.Identity;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Application.Tests.Manufacturing;

public class ManufacturingCommandTests
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
    public async Task CreateBomHeader_ShouldDetectCircularDependencyCycle()
    {
        using var db = CreateDbContext(out _);
        var handler = new CreateBomHeaderCommandHandler(db);

        var cyclicComponentId = Guid.NewGuid();
        var components = new List<BomComponentDto>
        {
            new BomComponentDto(cyclicComponentId, 1m, 100m, 0m),
            new BomComponentDto(cyclicComponentId, 2m, 98m, 2m) // Duplicate component causing cycle!
        };

        var command = new CreateBomHeaderCommand("Cyclic Assembly", "REV-ERR", components);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("BOM.CycleDetected", result.Error.Code);
    }

    [Fact]
    public async Task CreateBomHeader_ShouldSucceed_WhenValidDagHierarchy()
    {
        using var db = CreateDbContext(out _);
        var handler = new CreateBomHeaderCommandHandler(db);

        var components = new List<BomComponentDto>
        {
            new BomComponentDto(Guid.NewGuid(), 1m, 100m, 0m),
            new BomComponentDto(Guid.NewGuid(), 4m, 99m, 1m)
        };

        var command = new CreateBomHeaderCommand("Valid Assembly", "REV-A", components);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task RecordProductionOutput_ShouldSucceed_WhenValidQuantities()
    {
        var handler = new RecordProductionOutputCommandHandler();
        var command = new RecordProductionOutputCommand(Guid.NewGuid(), 10m, 1m, "Out of spec", 3.5m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }
}
