using System;

namespace YAERP.Domain.Entities.Warehouse;

public class WarehouseZone
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ZoneType { get; set; } = string.Empty; // Receiving, BulkStorage, Picking, Shipping, ColdStorage
}
