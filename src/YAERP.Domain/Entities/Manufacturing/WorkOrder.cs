using System;

namespace YAERP.Domain.Entities.Manufacturing;

public class WorkOrder
{
    public Guid Id { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public Guid BomHeaderId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal TargetQuantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public decimal ScrappedQuantity { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Released, InProcess, Completed, Cancelled
}
