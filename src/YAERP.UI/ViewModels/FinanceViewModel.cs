using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Finance.Commands.ExportInvoicePdf;

namespace YAERP.UI.ViewModels;

public record AccountDto(string AccountNumber, string Name, decimal Balance);
public record JournalEntryDto(string EntryNumber, string Description, bool IsPosted);
public record InvoiceListDto(Guid Id, string InvoiceNumber, decimal TotalAmount);

public partial class FinanceViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts = new();

    [ObservableProperty]
    private ObservableCollection<JournalEntryDto> _journalEntries = new();

    public FinanceViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadFinanceDataAsync()
    {
        await Task.Delay(200);
        Accounts.Clear();
        Accounts.Add(new AccountDto("1000", "Cash", 50000m));

        JournalEntries.Clear();
        JournalEntries.Add(new JournalEntryDto("JE-001", "Initial Deposit", true));
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
