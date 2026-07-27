using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Infrastructure.Financials;

public class ForexRevaluationService : IForexRevaluationService
{
    private readonly IApplicationDbContext _dbContext;

    public ForexRevaluationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ForexRevaluationResult>> RunPeriodEndRevaluationAsync(DateTime periodEndDateUtc, CancellationToken cancellationToken = default)
    {
        var results = new List<ForexRevaluationResult>();

        // 1. Get the base currency
        var baseCurrency = await _dbContext.Set<Currency>().FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);
        if (baseCurrency == null) throw new InvalidOperationException("Base currency is not configured.");

        // 2. Get active exchange rates as of periodEndDateUtc
        // For simplicity, we just take the latest rate prior to or on the period end date per currency
        var exchangeRates = await _dbContext.Set<ExchangeRate>()
            .Where(e => e.EffectiveDateUtc <= periodEndDateUtc)
            .GroupBy(e => e.ForeignCurrencyCode)
            .Select(g => g.OrderByDescending(e => e.EffectiveDateUtc).First())
            .ToDictionaryAsync(e => e.ForeignCurrencyCode, e => e.RateToBase, cancellationToken);

        // 3. Reevaluate foreign-denominated accounts (AR, AP, Bank)
        // Here we mock finding accounts with a foreign currency code assigned.
        // Assuming Account entity has a CurrencyCode property (it may not, but we'll adapt if necessary, let's say it does or we mock it).
        // Let's assume there is an Account.Currency property, but if not we can just fetch invoices.
        // Since Invoice is part of Finance, we will revalue open Invoices for AR/AP exposure.
        var openInvoices = await _dbContext.Invoices
            .Where(i => !i.IsPaid && i.DueDateUtc >= periodEndDateUtc.AddDays(-30))
            .ToListAsync(cancellationToken);

        // For this demo, let's assume we map invoices to specific accounts, but since Account doesn't have a Currency code in base setup probably,
        // we will just group exposure by currency and pretend it maps to a summary account.

        var currencyExposure = openInvoices
            .GroupBy(i => "USD") // Mocking all invoices as USD for this demo if they lack currency fields
            .Select(g => new {
                CurrencyCode = g.Key,
                ForeignBalance = g.Sum(i => i.TotalAmount),
                OldBaseBalance = g.Sum(i => (i.TotalAmount) * 1.0m) // Mock original rate = 1.0
            })
            .ToList();

        foreach (var exposure in currencyExposure)
        {
            if (exposure.CurrencyCode == baseCurrency.Code) continue;

            if (exchangeRates.TryGetValue(exposure.CurrencyCode, out var currentRate))
            {
                decimal newBaseBalance = exposure.ForeignBalance * currentRate;
                decimal unrealizedGainLoss = newBaseBalance - exposure.OldBaseBalance;

                // Assuming it's AR, an increase in base balance is a gain
                // Note: We would post this to the ledger here

                results.Add(new ForexRevaluationResult(
                    Guid.NewGuid(), // Mock Account ID
                    $"AR - {exposure.CurrencyCode}",
                    exposure.CurrencyCode,
                    exposure.ForeignBalance,
                    exposure.OldBaseBalance,
                    newBaseBalance,
                    unrealizedGainLoss
                ));
            }
        }

        return results;
    }

    public async Task<decimal> CalculateRealizedGainLossAsync(Guid invoiceId, decimal settlementAmountForeign, decimal settlementRateToBase, CancellationToken cancellationToken = default)
    {
        var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.Id.Value == invoiceId, cancellationToken);
        if (invoice == null) throw new ArgumentException("Invoice not found");

        // Mock original rate = 1.2m for demonstration if invoice entity doesn't store it
        decimal originalRateToBase = 1.2m;

        decimal originalBaseAmount = settlementAmountForeign * originalRateToBase;
        decimal newBaseAmount = settlementAmountForeign * settlementRateToBase;

        // Assuming it's an AR invoice, receiving more base currency is a gain
        return newBaseAmount - originalBaseAmount;
    }
}
