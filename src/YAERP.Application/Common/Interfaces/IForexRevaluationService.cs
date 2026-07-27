using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record ForexRevaluationResult(Guid AccountId, string AccountName, string CurrencyCode, decimal ForeignBalance, decimal OldBaseBalance, decimal NewBaseBalance, decimal UnrealizedGainLoss);

public interface IForexRevaluationService
{
    Task<List<ForexRevaluationResult>> RunPeriodEndRevaluationAsync(DateTime periodEndDateUtc, CancellationToken cancellationToken = default);
    Task<decimal> CalculateRealizedGainLossAsync(Guid invoiceId, decimal settlementAmountForeign, decimal settlementRateToBase, CancellationToken cancellationToken = default);
}
