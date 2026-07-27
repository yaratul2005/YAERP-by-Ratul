using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Manufacturing;

namespace YAERP.Infrastructure.Manufacturing;

public class MrpExplosionService : IMrpExplosionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IBomExplosionService _bomExplosionService;

    public MrpExplosionService(IApplicationDbContext dbContext, IBomExplosionService bomExplosionService)
    {
        _dbContext = dbContext;
        _bomExplosionService = bomExplosionService;
    }

    public async Task<List<MrpDemandResult>> RunMrpAsync(DateTime targetDate, CancellationToken cancellationToken = default)
    {
        var results = new List<MrpDemandResult>();

        // 1. Gather all open Sales Orders that represent Independent Demand (Gross Demand)
        // Note: IApplicationDbContext exposes DbSet<SalesOrder>
        var salesOrders = await _dbContext.SalesOrders
            .Include(so => so.Items)
            .Where(so => so.Status != YAERP.Domain.Sales.SalesOrderStatus.Fulfilled && so.Status != YAERP.Domain.Sales.SalesOrderStatus.Cancelled)
            .ToListAsync(cancellationToken);

        // Aggregate total independent demand per product
        var grossDemandMap = salesOrders
            .SelectMany(so => so.Items)
            .GroupBy(i => i.ProductId.Value)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        // Let's get a list of all unique products needed
        var allRequiredProducts = new HashSet<Guid>(grossDemandMap.Keys);

        // 2. Explode BOMs for any assemblies to get Dependent Demand for components
        var dependentDemandMap = new Dictionary<Guid, decimal>();
        foreach (var kvp in grossDemandMap)
        {
            var explodedItems = await _bomExplosionService.ExplodeBomAsync(kvp.Key, kvp.Value, cancellationToken);
            foreach (var item in explodedItems)
            {
                if (item.ComponentProductId == kvp.Key) continue; // It's the parent itself

                if (dependentDemandMap.ContainsKey(item.ComponentProductId))
                {
                    dependentDemandMap[item.ComponentProductId] += item.EffectiveQuantity;
                }
                else
                {
                    dependentDemandMap[item.ComponentProductId] = item.EffectiveQuantity;
                }
                allRequiredProducts.Add(item.ComponentProductId);
            }
        }

        // Combine Independent and Dependent Demand into Total Gross Demand
        var totalGrossDemand = new Dictionary<Guid, decimal>();
        foreach (var productId in allRequiredProducts)
        {
            decimal indDemand = grossDemandMap.ContainsKey(productId) ? grossDemandMap[productId] : 0m;
            decimal depDemand = dependentDemandMap.ContainsKey(productId) ? dependentDemandMap[productId] : 0m;
            totalGrossDemand[productId] = indDemand + depDemand;
        }

        // 3. For each product, calculate Net Requirements
        var productList = await _dbContext.Products
            .Where(p => allRequiredProducts.Contains(p.Id.Value))
            .ToDictionaryAsync(p => p.Id.Value, cancellationToken);

        foreach (var kvp in totalGrossDemand)
        {
            var productId = kvp.Key;
            var grossReq = kvp.Value;

            if (!productList.TryGetValue(productId, out var product)) continue;

            // Gather inventory on-hand
            // Assuming StockMovement handles OnHand calculation, or we have a quick way:
            var onHand = await _dbContext.StockMovements
                .Where(s => s.ProductId.Value == productId)
                .SumAsync(s => s.Quantity, cancellationToken);

            // Gather scheduled receipts (from Purchase Orders or existing Work Orders)
            var poReceipts = await _dbContext.PurchaseOrders
                .Include(po => po.Items)
                .Where(po => po.Status != YAERP.Domain.Purchasing.PurchaseOrderStatus.Received && po.Status != YAERP.Domain.Purchasing.PurchaseOrderStatus.Cancelled)
                .SelectMany(po => po.Items)
                .Where(i => i.ProductId.Value == productId)
                .SumAsync(i => i.Quantity, cancellationToken);

            var woReceipts = await _dbContext.WorkOrders
                .Where(wo => wo.PlannedEndDate <= targetDate && wo.Status != "Completed" && wo.Status != "Cancelled")
                // Need to match BOM AssemblyProductId to this product
                .Join(_dbContext.BomHeaders, wo => wo.BomHeaderId, bom => bom.Id, (wo, bom) => new { wo, bom })
                .Where(x => x.bom.AssemblyProductId == productId)
                .SumAsync(x => x.wo.TargetQuantity - x.wo.CompletedQuantity, cancellationToken);

            var scheduledReceipts = poReceipts + woReceipts;
            var safetyStock = 0m; // Fallback // Approximating SafetyStock as ReorderPoint

            // NetRequirement = max(0, GrossRequirement - OnHandStock - ScheduledReceipts + SafetyStock)
            decimal netReq = Math.Max(0, grossReq - onHand - scheduledReceipts + safetyStock);

            var plannedOrders = new List<PlannedOrder>();

            if (netReq > 0)
            {
                // Create planned order
                // Check if this product has a BOM (i.e., we manufacture it) or if we buy it
                var isManufactured = await _dbContext.BomHeaders.AnyAsync(b => b.AssemblyProductId == productId && b.IsActive, cancellationToken);

                string orderType = isManufactured ? "WorkOrder" : "PurchaseOrder";
                int leadTimeDays = 1; // Fallback // Assuming Product has LeadTimeDays, if not, fallback to 1

                var receiptDate = targetDate;
                var releaseDate = targetDate.AddDays(-leadTimeDays);

                plannedOrders.Add(new PlannedOrder(productId, netReq, releaseDate, receiptDate, orderType));
            }

            results.Add(new MrpDemandResult(productId, grossReq, onHand, scheduledReceipts, safetyStock, netReq, plannedOrders));
        }

        return results;
    }
}
