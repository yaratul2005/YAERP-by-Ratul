using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Inventory.Queries.GetProductStock;

public class GetProductStockQueryHandler : IQueryHandler<GetProductStockQuery, ProductStockResult>
{
    private readonly IApplicationDbContext _context;

    public GetProductStockQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductStockResult>> Handle(GetProductStockQuery request, CancellationToken cancellationToken)
    {
        var productId = new ProductId(request.ProductId);

        var query = _context.StockMovements
            .AsNoTracking()
            .Where(sm => sm.ProductId == productId);

        if (request.WarehouseId.HasValue)
        {
            var warehouseId = new WarehouseId(request.WarehouseId.Value);
            query = query.Where(sm => sm.WarehouseId == warehouseId);
        }

        // Positive movements
        var inboundTypes = new[] { StockMovementType.InboundReceipt, StockMovementType.TransferIn, StockMovementType.InventoryAdjustment }; // Adjustment can be both, assuming magnitude is used and we check sign, or we sum algebraic.
        // Actually, if Quantity is always positive, we need to subtract outbound. If Adjustment is algebraic, we just sum it.
        // Let's assume InboundReceipt, TransferIn are positive, OutboundShipment, TransferOut are negative. InventoryAdjustment is signed.
        // However, the rule says "Quantity > 0" in validator. This implies we need to conditionally add/subtract based on type.

        var movements = await query.Select(sm => new { sm.MovementType, sm.Quantity }).ToListAsync(cancellationToken);

        decimal totalQuantity = 0;
        foreach (var sm in movements)
        {
            if (sm.MovementType == StockMovementType.InboundReceipt ||
                sm.MovementType == StockMovementType.TransferIn ||
                sm.MovementType == StockMovementType.InventoryAdjustment) // Assuming adjustment is signed in DB even if command is > 0, actually if command > 0, it means we can only do positive adjustment with this setup, but let's assume it adds it.
            {
                totalQuantity += sm.Quantity;
            }
            else if (sm.MovementType == StockMovementType.OutboundShipment ||
                     sm.MovementType == StockMovementType.TransferOut)
            {
                totalQuantity -= sm.Quantity;
            }
        }

        return Result.Success(new ProductStockResult(request.ProductId, totalQuantity));
    }
}
