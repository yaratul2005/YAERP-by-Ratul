using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;

namespace YAERP.Application.Purchasing.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandHandler : ICommandHandler<ReceivePurchaseOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ReceivePurchaseOrderCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == new PurchaseOrderId(request.PurchaseOrderId), cancellationToken);

        if (po == null)
            return Result.Failure(new Error("PurchaseOrder.NotFound", "Purchase order not found.", ErrorType.NotFound));

        // We can approve it before receiving if needed, assuming the happy path it's approved.
        if (po.Status == PurchaseOrderStatus.Draft)
        {
            po.Approve();
        }

        try
        {
            po.MarkAsReceived();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("PurchaseOrder.InvalidState", ex.Message, ErrorType.Failure));
        }

        foreach (var item in po.Items)
        {
            var movement = StockMovement.Create(
                tenantId,
                item.ProductId,
                po.WarehouseId,
                StockMovementType.InboundReceipt,
                item.Quantity,
                item.UnitPrice,
                po.OrderNumber,
                _tenantContext.CurrentUserId);

            _context.StockMovements.Add(movement);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
