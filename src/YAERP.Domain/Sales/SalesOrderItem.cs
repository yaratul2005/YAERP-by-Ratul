using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Domain.Sales;

public class SalesOrderItem : Entity<SalesOrderItemId>
{
    internal SalesOrderItem(SalesOrderItemId id, ProductId productId, decimal quantity, decimal unitPrice) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private SalesOrderItem() { }

    public SalesOrderId SalesOrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
}
