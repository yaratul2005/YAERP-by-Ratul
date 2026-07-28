using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;

namespace YAERP.Infrastructure.Warehouse;

public class FefoPickingService : IFefoPickingService
{
    private readonly IApplicationDbContext _dbContext;

    public FefoPickingService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PickRouteStep>> GenerateFefoPickRouteAsync(Guid productId, decimal requiredQuantity, CancellationToken cancellationToken = default)
    {
        var steps = new List<PickRouteStep>();
        if (requiredQuantity <= 0) return steps;

        // In a real system, we'd query stock movements or an inventory summary table to find current stock per lot per bin.
        // For this demo, let's assume we have a table mapping LotId + BinId -> QuantityOnHand.
        // Since we don't have that yet, let's simulate querying the InventoryLot table ordered by ExpirationDate.

        var availableLots = await _dbContext.Set<YAERP.Domain.Entities.Warehouse.InventoryLot>()
            .Where(l => l.ProductId == productId)
            .OrderBy(l => l.ExpirationDate) // FEFO (First-Expired, First-Out)
            .ToListAsync(cancellationToken);

        decimal remainingToPick = requiredQuantity;

        foreach (var lot in availableLots)
        {
            if (remainingToPick <= 0) break;

            // Mock finding the bin and quantity for this lot
            // Ideally: await _dbContext.StockMovements.Where(x => x.LotId == lot.Id).SumAsync(x => x.Quantity);
            decimal qtyAvailableInLot = 100m; // Mock value

            if (qtyAvailableInLot <= 0) continue;

            decimal qtyToPick = Math.Min(qtyAvailableInLot, remainingToPick);
            remainingToPick -= qtyToPick;

            // Mock bin
            var binId = Guid.NewGuid();
            var binCode = "Z1-A01-B01-L01-P01";

            steps.Add(new PickRouteStep(binId, binCode, lot.Id, lot.LotNumber, qtyToPick));
        }

        // Ideally, we then sort the PickRouteStep by BinCode to create an efficient walking path
        return steps.OrderBy(s => s.BinCode).ToList();
    }

    public async Task<List<GenealogyTraceNode>> ForwardTraceAsync(string supplierLotRef, CancellationToken cancellationToken = default)
    {
        var trace = new List<GenealogyTraceNode>();

        // Find Lot
        var lot = await _dbContext.Set<YAERP.Domain.Entities.Warehouse.InventoryLot>()
            .FirstOrDefaultAsync(l => l.SupplierLotRef == supplierLotRef, cancellationToken);

        if (lot == null) return trace;

        trace.Add(new GenealogyTraceNode("PurchaseOrder", "PO-1234", lot.ManufactureDate, "Received from Supplier"));
        trace.Add(new GenealogyTraceNode("InventoryLot", lot.LotNumber, lot.ManufactureDate, "Assigned Internal Lot"));
        trace.Add(new GenealogyTraceNode("StockMovement", "MOV-001", lot.ManufactureDate.AddDays(1), "Stored in Bulk Zone"));
        trace.Add(new GenealogyTraceNode("SalesOrder", "SO-9999", lot.ManufactureDate.AddDays(10), "Shipped to Customer A"));

        return trace;
    }

    public async Task<List<GenealogyTraceNode>> BackwardTraceAsync(string salesOrderNumber, CancellationToken cancellationToken = default)
    {
        var trace = new List<GenealogyTraceNode>();

        // Reverse trace: Customer -> Shipment -> Bin -> Lot -> PO
        trace.Add(new GenealogyTraceNode("SalesOrder", salesOrderNumber, DateTime.UtcNow, "Shipped to Customer"));
        trace.Add(new GenealogyTraceNode("StockMovement", "MOV-002", DateTime.UtcNow.AddHours(-2), "Picked from Picking Zone"));
        trace.Add(new GenealogyTraceNode("InventoryLot", "LOT-998", DateTime.UtcNow.AddDays(-15), "Lot Identified"));
        trace.Add(new GenealogyTraceNode("PurchaseOrder", "PO-1234", DateTime.UtcNow.AddDays(-20), "Received from Supplier"));

        return trace;
    }
}
