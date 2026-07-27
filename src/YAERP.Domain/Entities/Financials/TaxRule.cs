using System;

namespace YAERP.Domain.Entities.Financials;

public class TaxRule
{
    public Guid Id { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal RatePercent { get; set; }
    public bool IsCompound { get; set; }
    public bool IsRecoverable { get; set; } // VAT input tax credit
}
