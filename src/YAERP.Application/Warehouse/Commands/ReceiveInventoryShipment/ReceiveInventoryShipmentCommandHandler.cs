using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Entities.Warehouse;
using YAERP.Domain.Identity;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Warehouse.Commands.ReceiveInventoryShipment;

public class ReceiveInventoryShipmentCommandHandler : ICommandHandler<ReceiveInventoryShipmentCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISlottingOptimizationService _slottingService;
    private readonly ITenantContext _tenantContext;

    public ReceiveInventoryShipmentCommandHandler(
        IApplicationDbContext dbContext,
        ISlottingOptimizationService slottingService,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _slottingService = slottingService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(ReceiveInventoryShipmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierOrderRef))
        {
            return Result.Failure<Guid>(new Error("Receiving.InvalidRef", "Supplier Order Reference is required.", ErrorType.Validation));
        }

        if (request.Quantity <= 0)
        {
            return Result.Failure<Guid>(new Error("Receiving.InvalidQuantity", "Quantity must be greater than zero.", ErrorType.Validation));
        }

        // Recommend optimal slotting bin using SlottingOptimizationService
        var recommendations = await _slottingService.RecommendBinsAsync(
            request.TargetWarehouseId,
            request.TotalVolume,
            request.TotalWeight,
            request.RequiresColdStorage,
            request.AbcClassification,
            cancellationToken);

        Guid assignedBinId = recommendations.FirstOrDefault()?.BinId ?? Guid.Empty;

        var tenantId = _tenantContext.CurrentTenantId ?? new TenantId(Guid.NewGuid());

        // Find or create product using Product.Create(...)
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Name == request.ProductName, cancellationToken);

        if (product == null)
        {
            product = Product.Create(
                tenantId,
                $"SKU-{request.ProductName.Replace(" ", "").ToUpper()}",
                null,
                request.ProductName,
                null,
                null,
                new UnitOfMeasureId(Guid.NewGuid()),
                request.UnitCost,
                request.UnitCost * 1.3m,
                false);

            _dbContext.Products.Add(product);
        }

        // Create Valuation Cost Layer
        var costLayer = new InventoryCostLayer
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id.Value,
            WarehouseId = request.TargetWarehouseId,
            LayerDateUtc = DateTime.UtcNow,
            OriginalQuantity = request.Quantity,
            RemainingQuantity = request.Quantity,
            UnitCost = request.UnitCost
        };
        _dbContext.Set<InventoryCostLayer>().Add(costLayer);

        // Record Inbound Stock Movement
        var movement = StockMovement.Create(
            tenantId,
            product.Id,
            new WarehouseId(request.TargetWarehouseId),
            StockMovementType.InboundReceipt,
            request.Quantity,
            request.UnitCost,
            request.SupplierOrderRef,
            _tenantContext.CurrentUserId);

        _dbContext.StockMovements.Add(movement);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(costLayer.Id);
    }
}
