using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record TaxCalculationResult(string TaxCode, decimal TaxAmount, bool IsRecoverable);

public record VatReturnSummary(decimal TotalOutputVat, decimal TotalInputVat, decimal NetVatLiability);

public interface ITaxComputationService
{
    Task<List<TaxCalculationResult>> CalculateTaxesAsync(decimal baseAmount, List<Guid> appliedTaxRuleIds, CancellationToken cancellationToken = default);
    Task<VatReturnSummary> GenerateVatReturnSummaryAsync(DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}
