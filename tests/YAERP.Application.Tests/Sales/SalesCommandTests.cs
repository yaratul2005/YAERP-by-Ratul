using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Sales.Commands.ConvertQuoteToSalesOrder;
using YAERP.Application.Sales.Commands.CreateSalesOrder;
using YAERP.Application.Sales.Commands.UpdateCustomerProfile;
using YAERP.Application.Sales.Commands.UpdateDealStage;
using YAERP.Domain.Identity;
using YAERP.Domain.Sales;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;

namespace YAERP.Application.Tests.Sales;

public class SalesCommandTests
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
    public async Task UpdateDealStage_ShouldReturnSuccess()
    {
        var handler = new UpdateDealStageCommandHandler();
        var command = new UpdateDealStageCommand(Guid.NewGuid(), "Proposal");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdateCustomerProfile_ShouldFail_WhenCustomerNotFound()
    {
        using var db = CreateDbContext(out _);
        var handler = new UpdateCustomerProfileCommandHandler(db);
        var command = new UpdateCustomerProfileCommand(Guid.NewGuid(), "Non-existent", "test@test.com", "123", "Addr", 1000m);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Customer.NotFound", result.Error.Code);
    }

    [Fact]
    public async Task ConvertQuoteToSalesOrder_ShouldCreateConfirmedOrder()
    {
        using var db = CreateDbContext(out var tenantContext);
        var customerId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var customer = Customer.Create(tenantContext.CurrentTenantId!, "Acme Test", "TAX1", "acme@test.com", "123", "Addr", 50000m);

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var handler = new ConvertQuoteToSalesOrderCommandHandler(db, tenantContext);
        var items = new List<SalesOrderItemDto>
        {
            new SalesOrderItemDto(Guid.NewGuid(), 2m, 150m)
        };

        var command = new ConvertQuoteToSalesOrderCommand(Guid.NewGuid(), customer.Id.Value, warehouseId, "SO-CNV-001", items);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var order = await db.SalesOrders.FirstOrDefaultAsync(so => so.OrderNumber == "SO-CNV-001");
        Assert.NotNull(order);
        Assert.Equal(SalesOrderStatus.Confirmed, order!.Status);
        Assert.Equal(300m, order.TotalAmount);
    }
}
