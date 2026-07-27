using System;

namespace YAERP.Domain.Entities.Financials;

public class ExchangeRate
{
    public Guid Id { get; set; }
    public string ForeignCurrencyCode { get; set; } = string.Empty;
    public decimal RateToBase { get; set; }
    public DateTime EffectiveDateUtc { get; set; }
}
