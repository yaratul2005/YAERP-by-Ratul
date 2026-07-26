using System;
using System.Linq;
using Xunit;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Purchasing.Events;

namespace YAERP.Domain.Tests.Purchasing;

public class PurchaseOrderTests
{
    [Fact]
    public void Create_Should_BeInDraftState()
    {
        var po = PurchaseOrder.Create(new TenantId(Guid.NewGuid()), new VendorId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "PO-123", null);
        Assert.Equal(PurchaseOrderStatus.Draft, po.Status);
    }

    [Fact]
    public void AddItem_WhenNotDraft_Should_ThrowException()
    {
        var po = PurchaseOrder.Create(new TenantId(Guid.NewGuid()), new VendorId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "PO-123", null);
        po.Approve();
        Assert.Throws<InvalidOperationException>(() => po.AddItem(new ProductId(Guid.NewGuid()), 1, 1));
    }

    [Fact]
    public void MarkAsReceived_Should_RaiseEvent()
    {
        var po = PurchaseOrder.Create(new TenantId(Guid.NewGuid()), new VendorId(Guid.NewGuid()), new WarehouseId(Guid.NewGuid()), "PO-123", null);
        po.Approve();
        po.MarkAsReceived();

        Assert.Equal(PurchaseOrderStatus.Received, po.Status);
        var evt = po.DomainEvents.OfType<PurchaseOrderReceivedEvent>().SingleOrDefault();
        Assert.NotNull(evt);
        Assert.Equal(po.Id, evt.PurchaseOrderId);
    }
}
