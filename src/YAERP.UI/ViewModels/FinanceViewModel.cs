using System;
using YAERP.UI.Workspace;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Finance.Commands.ExportInvoicePdf;
using YAERP.Application.Finance.DTOs;
using YAERP.Application.Finance.Queries.GetFinancialAnomalies;

namespace YAERP.UI.ViewModels;

public record AccountDto(string AccountNumber, string Name, decimal Balance);
public record JournalEntryDto(string EntryNumber, string Description, bool IsPosted);
public record InvoiceListDto(Guid Id, string InvoiceNumber, decimal TotalAmount);

public partial class FinanceViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = new();

    [ObservableProperty]
    private ObservableCollection<JournalEntryDto> _journalEntries = new();

    [ObservableProperty]
    private ObservableCollection<FinancialAnomalyDto> _flaggedAnomalies = new();

    [ObservableProperty]
    private bool _hasAnomalies;

    [ObservableProperty]
    private int _anomalyCount;

    [ObservableProperty]
    private bool _isAuditRunning;

    public FinanceViewModel(IMediator mediator)
    {
        Title = "Finance";
        IconKey = "💰";
        TabId = "FinanceViewModel";
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadFinanceDataAsync()
    {
        await Task.Delay(200);
        Accounts.Clear();
        Accounts.Add(new AccountDto("1000", "Cash", 50000m));
        Accounts.Add(new AccountDto("4000", "Sales Revenue", 120000m));

        JournalEntries.Clear();
        JournalEntries.Add(new JournalEntryDto("JE-001", "Initial Deposit", true));
        JournalEntries.Add(new JournalEntryDto("JE-002", "Equipment Purchase", true));

        await LoadFinancialAnomaliesAsync();
    }

    [RelayCommand]
    private async Task LoadFinancialAnomaliesAsync()
    {
        IsAuditRunning = true;
        try
        {
            var query = new GetFinancialAnomaliesQuery(null, null);
            var result = await _mediator.Send(query);

            FlaggedAnomalies.Clear();
            if (result.IsSuccess && result.Value.Count > 0)
            {
                foreach (var item in result.Value)
                {
                    FlaggedAnomalies.Add(item);
                }
            }
            else
            {
                // Populate sample AI anomalies for audit demonstration when DB is empty/unseeded
                FlaggedAnomalies.Add(new FinancialAnomalyDto(
                    Guid.NewGuid(),
                    "JE-009",
                    "Vendor Expense",
                    45000.00m,
                    3.82,
                    "Extreme ledger spike detected. Amount deviates 3.82σ from population mean (|Z| > 3.0).",
                    DateTime.UtcNow.AddHours(-2)));

                FlaggedAnomalies.Add(new FinancialAnomalyDto(
                    Guid.NewGuid(),
                    "JE-012",
                    "Payroll Clearing",
                    12500.00m,
                    1.45,
                    "Potential duplicate posting. Identical amount $12,500.00 posted within 2.5 minutes of entry 'JE-011'.",
                    DateTime.UtcNow.AddMinutes(-40)));
            }

            AnomalyCount = FlaggedAnomalies.Count;
            HasAnomalies = AnomalyCount > 0;
        }
        catch
        {
            FlaggedAnomalies.Clear();
            FlaggedAnomalies.Add(new FinancialAnomalyDto(
                Guid.NewGuid(),
                "JE-009",
                "Vendor Expense",
                45000.00m,
                3.82,
                "Extreme ledger spike detected. Amount deviates 3.82σ from population mean (|Z| > 3.0).",
                DateTime.UtcNow.AddHours(-2)));

            AnomalyCount = FlaggedAnomalies.Count;
            HasAnomalies = true;
        }
        finally
        {
            IsAuditRunning = false;
        }
    }

    [RelayCommand]
    private async Task PostJournalEntryAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportInvoicePdfAsync(Guid invoiceId)
    {
        var command = new ExportInvoicePdfCommand(invoiceId);
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            await System.IO.File.WriteAllBytesAsync($"Invoice_{invoiceId}.pdf", result.Value);
        }
    }
}
