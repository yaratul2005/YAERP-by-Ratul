using System;

namespace YAERP.Domain.Entities.Manufacturing;

public class BomItem
{
    public Guid Id { get; set; }
    public Guid BomHeaderId { get; set; }
    public Guid ComponentProductId { get; set; }
    public decimal QuantityPerAssembly { get; set; }
    public decimal ScrapFactorPercent { get; set; }
    public decimal YieldPercent { get; set; } = 100m;
    public int PositionNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsPhantomAssembly { get; set; }
}
