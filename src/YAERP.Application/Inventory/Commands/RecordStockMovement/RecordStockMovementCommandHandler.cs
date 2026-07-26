using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Inventory.Commands.RecordStockMovement;

public class RecordStockMovementCommandHandler : ICommandHandler<RecordStockMovementCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public RecordStockMovementCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(RecordStockMovementCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var productId = new ProductId(request.ProductId);
        var warehouseId = new WarehouseId(request.WarehouseId);

        var productExists = await _context.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
            return Result.Failure<Guid>(new Error("Product.NotFound", "Product not found.", ErrorType.NotFound));

        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId, cancellationToken);
        if (!warehouseExists)
            return Result.Failure<Guid>(new Error("Warehouse.NotFound", "Warehouse not found.", ErrorType.NotFound));

        var movement = StockMovement.Create(
            tenantId,
            productId,
            warehouseId,
            request.MovementType,
            request.Quantity,
            request.UnitCost,
            request.ReferenceNumber,
            _tenantContext.CurrentUserId);

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(movement.Id.Value);
    }
}
