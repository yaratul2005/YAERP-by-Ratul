using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Application.Finance.DTOs;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

/// <summary>
/// Statistical financial anomaly detection engine.
///
/// <para>
/// Two detection strategies are applied in a single pass over posted journal entries:
/// <list type="number">
///   <item>
///     <b>Z-score outlier detection</b> — flags any entry whose total debit amount
///     deviates more than ±3σ from the population mean (|Z| &gt; 3.0). These are extreme
///     ledger spikes that may indicate erroneous postings, fraud, or bulk misallocations.
///   </item>
///   <item>
///     <b>Near-duplicate detection</b> — flags any entry whose total amount is identical to
///     at least one other entry posted within a 5-minute window. Classic duplicate-payment
///     and double-booking signature.
///   </item>
/// </list>
/// </para>
/// <para>Read path: uses <c>AsNoTracking</c> throughout. No writes occur.</para>
/// </summary>
public sealed class GetFinancialAnomaliesQueryHandler
    : IQueryHandler<GetFinancialAnomaliesQuery, IReadOnlyList<FinancialAnomalyDto>>
{
    private const double ZScoreThreshold = 3.0;
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMinutes(5);

    private readonly IApplicationDbContext _context;

    public GetFinancialAnomaliesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<FinancialAnomalyDto>>> Handle(
        GetFinancialAnomaliesQuery request,
        CancellationToken cancellationToken)
    {
        // ── 1. Fetch posted journal entries in date window ─────────
        var query = _context.JournalEntries
            .AsNoTracking()
            .Include(j => j.Lines)
            .Where(j => j.IsPosted);

        if (request.StartDate.HasValue)
            query = query.Where(j => j.PostingDateUtc >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(j => j.PostingDateUtc <= request.EndDate.Value);

        var entries = await query
            .OrderBy(j => j.PostingDateUtc)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
            return Result.Success<IReadOnlyList<FinancialAnomalyDto>>(
                Array.Empty<FinancialAnomalyDto>());

        // ── 2. Resolve account names (batch lookup by AccountId) ───
        var accountIds = entries
            .SelectMany(j => j.Lines)
            .Select(l => l.AccountId)
            .Distinct()
            .ToHashSet();

        var accountNames = await _context.Accounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .Select(a => new { a.Id, a.Name })
            .ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        // ── 3. Project to flat working set ─────────────────────────
        // Amount = total debit per entry (balanced entries: Debit = Credit sum).
        var working = entries.Select(j => new
        {
            Entry = j,
            // Debit side total represents the gross transaction size.
            Amount = j.Lines.Sum(l => l.Debit),
            // Primary account = first line with a debit (or first line overall).
            PrimaryAccountName = j.Lines
                .Where(l => l.Debit > 0)
                .Select(l => accountNames.GetValueOrDefault(l.AccountId, "Unknown Account"))
                .FirstOrDefault()
                ?? accountNames.GetValueOrDefault(
                    j.Lines.FirstOrDefault()?.AccountId!,
                    "Unknown Account")
        }).ToList();

        // ── 4. Z-score statistics ──────────────────────────────────
        // Use population std-dev (all posted entries are the full population).
        double mean = (double)working.Average(w => w.Amount);
        double variance = working.Average(w =>
            Math.Pow((double)w.Amount - mean, 2));
        double sigma = Math.Sqrt(variance);

        var anomalies = new List<FinancialAnomalyDto>();

        // ── 5. Z-score outlier detection ──────────────────────────
        if (sigma > 0)
        {
            foreach (var item in working)
            {
                double z = Math.Abs(((double)item.Amount - mean) / sigma);
                if (z > ZScoreThreshold)
                {
                    anomalies.Add(new FinancialAnomalyDto(
                        JournalEntryId: item.Entry.Id.Value,
                        EntryNumber: item.Entry.EntryNumber,
                        AccountName: item.PrimaryAccountName,
                        Amount: item.Amount,
                        ZScore: Math.Round(z, 4),
                        AnomalyReason:
                            $"Extreme ledger spike detected. Amount deviates {z:F2}σ from population mean " +
                            $"(μ={mean:F2}, σ={sigma:F2}). Threshold: |Z| > {ZScoreThreshold}.",
                        PostingDateUtc: item.Entry.PostingDateUtc));
                }
            }
        }

        // ── 6. Near-duplicate detection within 5-minute window ─────
        // Compare every pair where Amount matches exactly and entries are < 5 min apart.
        // O(n²) — acceptable for audit data sets (typically < 50k entries per scan).
        var alreadyFlagged = new HashSet<Guid>();

        for (int i = 0; i < working.Count; i++)
        {
            for (int j = i + 1; j < working.Count; j++)
            {
                var a = working[i];
                var b = working[j];

                if (a.Amount != b.Amount) continue;

                var timeDelta = (b.Entry.PostingDateUtc - a.Entry.PostingDateUtc).Duration();
                if (timeDelta > DuplicateWindow) break; // list is ordered by date

                // Flag both entries unless already reported.
                if (alreadyFlagged.Add(a.Entry.Id.Value))
                {
                    anomalies.Add(new FinancialAnomalyDto(
                        JournalEntryId: a.Entry.Id.Value,
                        EntryNumber: a.Entry.EntryNumber,
                        AccountName: a.PrimaryAccountName,
                        Amount: a.Amount,
                        ZScore: sigma > 0
                            ? Math.Round(Math.Abs(((double)a.Amount - mean) / sigma), 4)
                            : 0,
                        AnomalyReason:
                            $"Potential duplicate posting. Identical amount {a.Amount:C2} posted within " +
                            $"{timeDelta.TotalMinutes:F1} minutes of entry '{b.Entry.EntryNumber}'.",
                        PostingDateUtc: a.Entry.PostingDateUtc));
                }

                if (alreadyFlagged.Add(b.Entry.Id.Value))
                {
                    anomalies.Add(new FinancialAnomalyDto(
                        JournalEntryId: b.Entry.Id.Value,
                        EntryNumber: b.Entry.EntryNumber,
                        AccountName: b.PrimaryAccountName,
                        Amount: b.Amount,
                        ZScore: sigma > 0
                            ? Math.Round(Math.Abs(((double)b.Amount - mean) / sigma), 4)
                            : 0,
                        AnomalyReason:
                            $"Potential duplicate posting. Identical amount {b.Amount:C2} posted within " +
                            $"{timeDelta.TotalMinutes:F1} minutes of entry '{a.Entry.EntryNumber}'.",
                        PostingDateUtc: b.Entry.PostingDateUtc));
                }
            }
        }

        // Return ordered by severity (Z-score descending) then date.
        IReadOnlyList<FinancialAnomalyDto> result = anomalies
            .OrderByDescending(a => Math.Abs(a.ZScore))
            .ThenBy(a => a.PostingDateUtc)
            .ToList();

        return Result.Success(result);
    }
}
