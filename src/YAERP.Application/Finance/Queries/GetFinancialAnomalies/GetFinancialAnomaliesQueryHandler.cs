using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

public class GetFinancialAnomaliesQueryHandler : IQueryHandler<GetFinancialAnomaliesQuery, List<FinancialAnomalyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFinancialAnomaliesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FinancialAnomalyDto>>> Handle(GetFinancialAnomaliesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _context.JournalEntries
            .AsNoTracking()
            .Include(j => j.Lines)
            .ToListAsync(cancellationToken);

        if (!entries.Any())
        {
            return Result.Success(new List<FinancialAnomalyDto>());
        }

        var entryAmounts = entries.Select(j => new
        {
            Entry = j,
            TotalAmount = j.Lines.Sum(l => l.Debit) // Since balanced, Debit = Credit
        }).ToList();

        var average = entryAmounts.Average(e => e.TotalAmount);
        var sumOfSquaresOfDifferences = entryAmounts.Select(val => (val.TotalAmount - average) * (val.TotalAmount - average)).Sum();
        var standardDeviation = (decimal)Math.Sqrt((double)(sumOfSquaresOfDifferences / entryAmounts.Count));

        var anomalies = new List<FinancialAnomalyDto>();

        if (standardDeviation == 0) return Result.Success(anomalies);

        foreach (var item in entryAmounts)
        {
            var zScore = Math.Abs((item.TotalAmount - average) / standardDeviation);
            if (zScore > request.ThresholdZScore)
            {
                anomalies.Add(new FinancialAnomalyDto(
                    item.Entry.Id.Value,
                    item.Entry.EntryNumber,
                    zScore,
                    $"Extremely high transaction volume relative to historical mean. (Z-Score: {zScore:F2})"));
            }
        }

        return Result.Success(anomalies);
    }
}
