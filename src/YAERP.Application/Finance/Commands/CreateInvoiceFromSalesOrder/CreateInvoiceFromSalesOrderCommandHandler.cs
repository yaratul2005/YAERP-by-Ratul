using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;
using YAERP.Domain.Sales;

namespace YAERP.Application.Finance.Commands.CreateInvoiceFromSalesOrder;

public class CreateInvoiceFromSalesOrderCommandHandler : ICommandHandler<CreateInvoiceFromSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateInvoiceFromSalesOrderCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateInvoiceFromSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.Invoices.AnyAsync(i => i.InvoiceNumber == request.InvoiceNumber, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("Invoice.Duplicate", "Invoice number must be unique.", ErrorType.Conflict));

        var soId = new SalesOrderId(request.SalesOrderId);
        var so = await _context.SalesOrders
            .FirstOrDefaultAsync(s => s.Id == soId, cancellationToken);

        if (so == null)
            return Result.Failure<Guid>(new Error("SalesOrder.NotFound", "Sales order not found.", ErrorType.NotFound));

        if (so.Status != SalesOrderStatus.Fulfilled)
            return Result.Failure<Guid>(new Error("SalesOrder.NotFulfilled", "Only fulfilled sales orders can be invoiced.", ErrorType.Failure));

        var invoice = Invoice.CreateForSalesOrder(
            tenantId,
            request.InvoiceNumber,
            soId,
            so.TotalAmount,
            request.DueDateUtc);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(invoice.Id.Value);
    }
}
