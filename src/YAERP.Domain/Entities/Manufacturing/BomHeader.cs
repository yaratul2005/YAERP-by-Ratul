using System;
using System.Collections.Generic;

namespace YAERP.Domain.Entities.Manufacturing;

public class BomHeader
{
    public Guid Id { get; set; }
    public Guid AssemblyProductId { get; set; }
    public string RevisionCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal BaseQuantity { get; set; } = 1m;
    public string Version { get; set; } = string.Empty;
    public ICollection<BomItem> Components { get; set; } = new List<BomItem>();
}
