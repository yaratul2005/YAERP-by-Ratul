using System;

namespace YAERP.Application.Finance.DTOs;

/// <summary>
/// Carries anomaly detection results for a single journal entry line.
/// Immutable record — no mutation after construction.
/// </summary>
public sealed record FinancialAnomalyDto(
    Guid JournalEntryId,
    string EntryNumber,
    string AccountName,
    decimal Amount,
    double ZScore,
    string AnomalyReason,
    DateTime PostingDateUtc);
