using System;

namespace YAERP.Domain.Entities.Warehouse;

public class ProductSerialNumber
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid? CurrentBinId { get; set; }
    public string Status { get; set; } = "InStock"; // InStock, Reserved, Shipped, Quarantine
}
