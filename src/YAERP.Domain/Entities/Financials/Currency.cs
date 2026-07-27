namespace YAERP.Domain.Entities.Financials;

public class Currency
{
    public string Code { get; set; } = string.Empty; // ISO 4217, e.g., "USD", "EUR", "BDT"
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
}
