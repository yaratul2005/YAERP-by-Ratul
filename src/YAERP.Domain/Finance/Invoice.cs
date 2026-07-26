using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;
using YAERP.Domain.Purchasing;
using YAERP.Domain.Sales;
using YAERP.Domain.Finance.Events;

namespace YAERP.Domain.Finance;

public class Invoice : AggregateRoot<InvoiceId>
{
    private Invoice(InvoiceId id, TenantId tenantId, string invoiceNumber, SalesOrderId? salesOrderId, PurchaseOrderId? purchaseOrderId, decimal totalAmount, DateTime dueDateUtc, bool isPaid) : base(id)
    {
        TenantId = tenantId;
        InvoiceNumber = invoiceNumber;
        SalesOrderId = salesOrderId;
        PurchaseOrderId = purchaseOrderId;
        TotalAmount = totalAmount;
        DueDateUtc = dueDateUtc;
        IsPaid = isPaid;
    }

    private Invoice() { }

    public TenantId TenantId { get; private set; } = default!;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public SalesOrderId? SalesOrderId { get; private set; }
    public PurchaseOrderId? PurchaseOrderId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime DueDateUtc { get; private set; }
    public bool IsPaid { get; private set; }

    public static Invoice CreateForSalesOrder(TenantId tenantId, string invoiceNumber, SalesOrderId salesOrderId, decimal totalAmount, DateTime dueDateUtc)
    {
        var invoice = new Invoice(new InvoiceId(Guid.NewGuid()), tenantId, invoiceNumber, salesOrderId, null, totalAmount, dueDateUtc, false);
        invoice.AddDomainEvent(new InvoiceGeneratedEvent(invoice.Id));
        return invoice;
    }

    public static Invoice CreateForPurchaseOrder(TenantId tenantId, string invoiceNumber, PurchaseOrderId purchaseOrderId, decimal totalAmount, DateTime dueDateUtc)
    {
        var invoice = new Invoice(new InvoiceId(Guid.NewGuid()), tenantId, invoiceNumber, null, purchaseOrderId, totalAmount, dueDateUtc, false);
        invoice.AddDomainEvent(new InvoiceGeneratedEvent(invoice.Id));
        return invoice;
    }
}
