using System;

namespace YAERP.Domain.Entities.Financials;

public class DepreciationScheduleEntry
{
    public Guid Id { get; set; }
    public Guid FixedAssetId { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }

    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciationAfter { get; set; }
    public decimal BookValueAfter { get; set; }

    public bool IsPosted { get; set; }
    public DateTime? PostedAtUtc { get; set; }
}
