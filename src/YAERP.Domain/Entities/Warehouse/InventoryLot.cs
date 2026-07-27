using System;

namespace YAERP.Domain.Entities.Warehouse;

public class InventoryLot
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public DateTime ManufactureDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? SupplierLotRef { get; set; }
}
