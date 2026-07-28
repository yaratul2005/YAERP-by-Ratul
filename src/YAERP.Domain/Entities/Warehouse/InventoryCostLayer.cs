using System;

namespace YAERP.Domain.Entities.Warehouse;

public class InventoryCostLayer
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime LayerDateUtc { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
