using System;

namespace YAERP.Domain.Entities.Manufacturing;

public class RoutingStep
{
    public Guid Id { get; set; }
    public Guid BomHeaderId { get; set; }
    public Guid WorkCenterId { get; set; }
    public int SequenceOrder { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public decimal SetupTimeHours { get; set; }
    public decimal RunTimeHoursPerUnit { get; set; }
}
