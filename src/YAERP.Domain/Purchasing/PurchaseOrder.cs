using System;
using System.Collections.Generic;
using System.Linq;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing.Events;

namespace YAERP.Domain.Purchasing;

public class PurchaseOrder : AggregateRoot<PurchaseOrderId>
{
    private readonly List<PurchaseOrderItem> _items = new();

    private PurchaseOrder(PurchaseOrderId id, TenantId tenantId, VendorId vendorId, WarehouseId warehouseId, string orderNumber, DateTime orderDateUtc, DateTime? expectedDeliveryDateUtc) : base(id)
    {
        TenantId = tenantId;
        VendorId = vendorId;
        WarehouseId = warehouseId;
        OrderNumber = orderNumber;
        Status = PurchaseOrderStatus.Draft;
        OrderDateUtc = orderDateUtc;
        ExpectedDeliveryDateUtc = expectedDeliveryDateUtc;
    }

    private PurchaseOrder() { }

    public TenantId TenantId { get; private set; } = default!;
    public VendorId VendorId { get; private set; } = default!;
    public WarehouseId WarehouseId { get; private set; } = default!;
    public string OrderNumber { get; private set; } = string.Empty;
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime OrderDateUtc { get; private set; }
    public DateTime? ExpectedDeliveryDateUtc { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Quantity * i.UnitPrice);

    public static PurchaseOrder Create(TenantId tenantId, VendorId vendorId, WarehouseId warehouseId, string orderNumber, DateTime? expectedDeliveryDateUtc)
    {
        return new PurchaseOrder(new PurchaseOrderId(Guid.NewGuid()), tenantId, vendorId, warehouseId, orderNumber, DateTime.UtcNow, expectedDeliveryDateUtc);
    }

    public void AddItem(ProductId productId, decimal quantity, decimal unitPrice)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Can only add items to Draft orders.");

        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.");

        _items.Add(new PurchaseOrderItem(new PurchaseOrderItemId(Guid.NewGuid()), productId, quantity, unitPrice));
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft orders can be approved.");

        Status = PurchaseOrderStatus.Approved;
    }

    public void MarkAsReceived()
    {
        if (Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException("Only Approved orders can be received.");

        Status = PurchaseOrderStatus.Received;
        AddDomainEvent(new PurchaseOrderReceivedEvent(Id));
    }
}
