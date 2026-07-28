using System;

namespace YAERP.Domain.Entities.Warehouse;

public class WarehouseBin
{
    public Guid Id { get; set; }
    public Guid ZoneId { get; set; }
    public string BinCode { get; set; } = string.Empty; // Format: ZONE-AISLE-BAY-LEVEL-POS
    public string Aisle { get; set; } = string.Empty;
    public string Bay { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public decimal MaxVolumeCubicMeters { get; set; }
    public decimal MaxWeightKg { get; set; }
    public bool IsLocked { get; set; }
    public bool IsTemperatureControlled { get; set; }
}
