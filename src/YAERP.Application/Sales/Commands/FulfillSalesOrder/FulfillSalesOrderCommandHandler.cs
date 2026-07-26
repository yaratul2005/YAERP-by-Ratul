using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;

namespace YAERP.Application.Sales.Commands.FulfillSalesOrder;

public class FulfillSalesOrderCommandHandler : ICommandHandler<FulfillSalesOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public FulfillSalesOrderCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(FulfillSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var so = await _context.SalesOrders
            .FirstOrDefaultAsync(s => s.Id == new SalesOrderId(request.SalesOrderId), cancellationToken);

        if (so == null)
            return Result.Failure(new Error("SalesOrder.NotFound", "Sales order not found.", ErrorType.NotFound));

        if (so.Status == SalesOrderStatus.Draft)
        {
            so.Confirm();
        }

        // Check if sufficient QuantityOnHand exists
        foreach (var item in so.Items)
        {
            var stockMovements = await _context.StockMovements
                .AsNoTracking()
                .Where(sm => sm.ProductId == item.ProductId && sm.WarehouseId == so.WarehouseId)
                .Select(sm => new { sm.MovementType, sm.Quantity })
                .ToListAsync(cancellationToken);

            decimal totalQuantity = 0;
            foreach (var sm in stockMovements)
            {
                if (sm.MovementType == StockMovementType.InboundReceipt ||
                    sm.MovementType == StockMovementType.TransferIn ||
                    sm.MovementType == StockMovementType.InventoryAdjustment)
                {
                    totalQuantity += sm.Quantity;
                }
                else if (sm.MovementType == StockMovementType.OutboundShipment ||
                         sm.MovementType == StockMovementType.TransferOut)
                {
                    totalQuantity -= sm.Quantity;
                }
            }

            if (totalQuantity < item.Quantity)
            {
                return Result.Failure(new Error("SalesOrder.InsufficientStock", $"Insufficient stock for ProductId: {item.ProductId.Value}. Available: {totalQuantity}, Required: {item.Quantity}.", ErrorType.Conflict));
            }
        }

        try
        {
            so.Fulfill();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("SalesOrder.InvalidState", ex.Message, ErrorType.Failure));
        }

        foreach (var item in so.Items)
        {
            var movement = StockMovement.Create(
                tenantId,
                item.ProductId,
                so.WarehouseId,
                StockMovementType.OutboundShipment,
                item.Quantity,
                item.UnitPrice,
                so.OrderNumber,
                _tenantContext.CurrentUserId);

            _context.StockMovements.Add(movement);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
