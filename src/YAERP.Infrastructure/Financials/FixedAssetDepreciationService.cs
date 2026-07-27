using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Infrastructure.Financials;

public class FixedAssetDepreciationService : IFixedAssetDepreciationService
{
    private readonly IApplicationDbContext _dbContext;

    public FixedAssetDepreciationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DepreciationScheduleEntry>> GenerateScheduleAsync(Guid fixedAssetId, CancellationToken cancellationToken = default)
    {
        var asset = await _dbContext.Set<FixedAsset>().FirstOrDefaultAsync(a => a.Id == fixedAssetId, cancellationToken);
        if (asset == null) throw new ArgumentException("Asset not found.", nameof(fixedAssetId));
        if (asset.UsefulLifeYears <= 0) throw new InvalidOperationException("Useful life must be greater than zero.");

        var schedule = new List<DepreciationScheduleEntry>();

        decimal cost = asset.AcquisitionCost;
        decimal salvage = asset.SalvageValue;
        int life = asset.UsefulLifeYears;

        decimal accumDep = 0m;
        decimal currentBookValue = cost;
        int startYear = asset.InServiceDate.Year;
        // Simplified schedule generation assuming annual depreciation evenly spread across months
        // Here we generate yearly schedules and divide by 12, or just generate yearly amounts
        // For simplicity in mathematical algorithms, we calculate the yearly depreciation D_t first.

        for (int t = 1; t <= life; t++)
        {
            decimal yearlyDepreciation = 0m;

            if (asset.DepreciationMethod == "StraightLine")
            {
                yearlyDepreciation = (cost - salvage) / life;
            }
            else if (asset.DepreciationMethod == "DoubleDeclining")
            {
                decimal ddbRate = 2m / life;
                yearlyDepreciation = ddbRate * currentBookValue;

                // Switch to straight line if DDB is lower
                decimal remainingLife = life - t + 1;
                decimal slAmount = (currentBookValue - salvage) / remainingLife;

                if (yearlyDepreciation < slAmount)
                {
                    yearlyDepreciation = slAmount;
                }
            }
            else if (asset.DepreciationMethod == "SumOfYearsDigits")
            {
                int sydDenominator = (life * (life + 1)) / 2;
                int sydNumerator = life - t + 1;
                yearlyDepreciation = ((decimal)sydNumerator / sydDenominator) * (cost - salvage);
            }
            else
            {
                throw new NotSupportedException($"Depreciation method {asset.DepreciationMethod} is not supported.");
            }

            // Enforce salvage value floor
            if (currentBookValue - yearlyDepreciation < salvage)
            {
                yearlyDepreciation = currentBookValue - salvage;
            }

            // Distribute the yearly amount over 12 months (or just generate yearly entries for demo)
            // Let's generate monthly entries
            decimal monthlyDepreciation = yearlyDepreciation / 12m;
            for (int m = 1; m <= 12; m++)
            {
                if (currentBookValue - monthlyDepreciation < salvage)
                {
                    monthlyDepreciation = currentBookValue - salvage;
                }

                accumDep += monthlyDepreciation;
                currentBookValue -= monthlyDepreciation;

                if (monthlyDepreciation > 0)
                {
                    schedule.Add(new DepreciationScheduleEntry
                    {
                        Id = Guid.NewGuid(),
                        FixedAssetId = fixedAssetId,
                        PeriodYear = startYear + t - 1,
                        PeriodMonth = m, // Assuming calendar year starts Jan, but depends on InServiceDate month in reality
                        DepreciationAmount = monthlyDepreciation,
                        AccumulatedDepreciationAfter = accumDep,
                        BookValueAfter = currentBookValue,
                        IsPosted = false
                    });
                }
            }

            if (currentBookValue <= salvage) break;
        }

        return schedule;
    }

    public async Task PostDepreciationEntriesAsync(int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        var entriesToPost = await _dbContext.Set<DepreciationScheduleEntry>()
            .Where(e => e.PeriodYear == periodYear && e.PeriodMonth == periodMonth && !e.IsPosted)
            .ToListAsync(cancellationToken);

        foreach (var entry in entriesToPost)
        {
            var asset = await _dbContext.Set<FixedAsset>().FirstOrDefaultAsync(a => a.Id == entry.FixedAssetId, cancellationToken);
            if (asset != null)
            {
                asset.AccumulatedDepreciation += entry.DepreciationAmount;
                asset.BookValue -= entry.DepreciationAmount;

                if (asset.BookValue <= asset.SalvageValue)
                {
                    asset.Status = "FullyDepreciated";
                }
            }

            entry.IsPosted = true;
            entry.PostedAtUtc = DateTime.UtcNow;

            // Here we would also generate JournalEntry records into YAERP.Domain.Finance.JournalEntry
        }

        // Wait for caller to call SaveChangesAsync
    }
}
