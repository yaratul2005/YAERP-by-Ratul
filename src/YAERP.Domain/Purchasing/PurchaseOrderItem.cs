using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Domain.Purchasing;

public class PurchaseOrderItem : Entity<PurchaseOrderItemId>
{
    internal PurchaseOrderItem(PurchaseOrderItemId id, ProductId productId, decimal quantity, decimal unitPrice) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private PurchaseOrderItem() { }

    public PurchaseOrderId PurchaseOrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
}
