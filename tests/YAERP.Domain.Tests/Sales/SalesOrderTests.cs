using System;
using System.Linq;
using Xunit;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;
using YAERP.Domain.Sales.Events;

namespace YAERP.Domain.Tests.Sales;

public class SalesOrderTests
{
    [Fact]
    public void Create_Should_BeInDraftState()
    {
        var so = SalesOrder.Create(new TenantId(Guid.NewGuid()), new CustomerId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "SO-123");
        Assert.Equal(SalesOrderStatus.Draft, so.Status);
    }

    [Fact]
    public void Confirm_Should_ChangeStatusToConfirmed()
    {
        var so = SalesOrder.Create(new TenantId(Guid.NewGuid()), new CustomerId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "SO-123");
        so.Confirm();
        Assert.Equal(SalesOrderStatus.Confirmed, so.Status);
    }

    [Fact]
    public void Fulfill_Should_RaiseEvent()
    {
        var so = SalesOrder.Create(new TenantId(Guid.NewGuid()), new CustomerId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "SO-123");
        so.Confirm();
        so.Fulfill();

        Assert.Equal(SalesOrderStatus.Fulfilled, so.Status);
        var evt = so.DomainEvents.OfType<SalesOrderFulfilledEvent>().SingleOrDefault();
        Assert.NotNull(evt);
        Assert.Equal(so.Id, evt.SalesOrderId);
    }
}
