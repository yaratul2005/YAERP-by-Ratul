using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;
using YAERP.Application.Finance.DTOs;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

/// <summary>
/// CQRS query to detect statistical financial anomalies (Z-score outliers and
/// duplicate entries) across posted journal entries within an optional date window.
/// </summary>
/// <param name="StartDate">Inclusive lower bound on <c>PostingDateUtc</c>. Null = unbounded.</param>
/// <param name="EndDate">Inclusive upper bound on <c>PostingDateUtc</c>. Null = unbounded.</param>
public record GetFinancialAnomaliesQuery(
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<IReadOnlyList<FinancialAnomalyDto>>;
