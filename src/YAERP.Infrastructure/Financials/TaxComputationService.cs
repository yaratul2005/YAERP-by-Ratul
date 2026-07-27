using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Infrastructure.Financials;

public class TaxComputationService : ITaxComputationService
{
    private readonly IApplicationDbContext _dbContext;

    public TaxComputationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TaxCalculationResult>> CalculateTaxesAsync(decimal baseAmount, List<Guid> appliedTaxRuleIds, CancellationToken cancellationToken = default)
    {
        var rules = await _dbContext.Set<TaxRule>()
            .Where(r => appliedTaxRuleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var results = new List<TaxCalculationResult>();
        decimal currentCompoundBase = baseAmount;

        // Process standard taxes first
        foreach (var rule in rules.Where(r => !r.IsCompound))
        {
            decimal taxAmount = baseAmount * (rule.RatePercent / 100m);
            results.Add(new TaxCalculationResult(rule.TaxCode, taxAmount, rule.IsRecoverable));
            currentCompoundBase += taxAmount;
        }

        // Process compound taxes on the new accumulated base
        foreach (var rule in rules.Where(r => r.IsCompound))
        {
            decimal taxAmount = currentCompoundBase * (rule.RatePercent / 100m);
            results.Add(new TaxCalculationResult(rule.TaxCode, taxAmount, rule.IsRecoverable));
            currentCompoundBase += taxAmount;
        }

        return results;
    }

    public async Task<VatReturnSummary> GenerateVatReturnSummaryAsync(DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        // 1. Calculate Output VAT (Collected on Sales)
        // Mocking: We'd typically query a dedicated TaxEntry ledger or the Journal entries mapped to VAT Payable account.
        // For this demo, let's assume we sum 15% of all Sales Invoices
        var salesInvoices = await _dbContext.Invoices
            .Where(i => i.SalesOrderId != null && i.DueDateUtc >= periodStart && i.DueDateUtc <= periodEnd)
            .ToListAsync(cancellationToken);

        decimal totalOutputVat = salesInvoices.Sum(i => i.TotalAmount * 0.15m); // Flat 15% assumption

        // 2. Calculate Input VAT (Paid on Purchases)
        var purchaseInvoices = await _dbContext.Invoices
            .Where(i => i.PurchaseOrderId != null && i.DueDateUtc >= periodStart && i.DueDateUtc <= periodEnd)
            .ToListAsync(cancellationToken);

        decimal totalInputVat = purchaseInvoices.Sum(i => i.TotalAmount * 0.15m); // Flat 15% assumption

        // 3. Net VAT Liability = Output - Input
        decimal netLiability = totalOutputVat - totalInputVat;

        return new VatReturnSummary(totalOutputVat, totalInputVat, netLiability);
    }
}
