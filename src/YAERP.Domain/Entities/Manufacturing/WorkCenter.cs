using System;

namespace YAERP.Domain.Entities.Manufacturing;

public class WorkCenter
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal DailyCapacityHours { get; set; }
    public decimal EfficiencyRatePercent { get; set; } = 100m;
    public decimal HourlyCost { get; set; }
}
