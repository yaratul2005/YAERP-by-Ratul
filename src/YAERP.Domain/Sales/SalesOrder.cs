using System;
using System.Collections.Generic;
using System.Linq;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales.Events;

namespace YAERP.Domain.Sales;

public class SalesOrder : AggregateRoot<SalesOrderId>
{
    private readonly List<SalesOrderItem> _items = new();

    private SalesOrder(SalesOrderId id, TenantId tenantId, CustomerId customerId, WarehouseId warehouseId, string orderNumber, DateTime orderDateUtc) : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        WarehouseId = warehouseId;
        OrderNumber = orderNumber;
        Status = SalesOrderStatus.Draft;
        OrderDateUtc = orderDateUtc;
    }

    private SalesOrder() { }

    public TenantId TenantId { get; private set; } = default!;
    public CustomerId CustomerId { get; private set; } = default!;
    public WarehouseId WarehouseId { get; private set; } = default!;
    public string OrderNumber { get; private set; } = string.Empty;
    public SalesOrderStatus Status { get; private set; }
    public DateTime OrderDateUtc { get; private set; }

    public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Quantity * i.UnitPrice);

    public static SalesOrder Create(TenantId tenantId, CustomerId customerId, WarehouseId warehouseId, string orderNumber)
    {
        return new SalesOrder(new SalesOrderId(Guid.NewGuid()), tenantId, customerId, warehouseId, orderNumber, DateTime.UtcNow);
    }

    public void AddItem(ProductId productId, decimal quantity, decimal unitPrice)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Can only add items to Draft orders.");

        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.");

        _items.Add(new SalesOrderItem(new SalesOrderItemId(Guid.NewGuid()), productId, quantity, unitPrice));
    }

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft orders can be confirmed.");

        Status = SalesOrderStatus.Confirmed;
    }

    public void Fulfill()
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException("Only Confirmed orders can be fulfilled.");

        Status = SalesOrderStatus.Fulfilled;
        AddDomainEvent(new SalesOrderFulfilledEvent(Id));
    }
}
