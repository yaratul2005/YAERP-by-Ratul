using System;

namespace YAERP.Domain.Entities.Financials;

public class FixedAsset
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Machinery, Vehicles, IT_Hardware, Real_Estate

    public decimal AcquisitionCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }
    public DateTime InServiceDate { get; set; }

    public string DepreciationMethod { get; set; } = "StraightLine"; // StraightLine, DoubleDeclining, SumOfYearsDigits

    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public string Status { get; set; } = "Active"; // Active, FullyDepreciated, Disposed
}
