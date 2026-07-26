using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

public record FinancialAnomalyDto(Guid JournalEntryId, string EntryNumber, decimal DeviationScore, string Warning);

public record GetFinancialAnomaliesQuery(int ThresholdZScore = 3) : IQuery<List<FinancialAnomalyDto>>;
