using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Financials.Commands.PostJournalEntry;

namespace YAERP.UI.ViewModels.Modals;

public partial class JournalEntryLineViewModel : ObservableObject
{
    [ObservableProperty]
    private string _accountCode = "1010";

    [ObservableProperty]
    private string _accountName = "1010 - Cash on Hand";

    [ObservableProperty]
    private string _description = "Journal ledger entry line";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineAmount))]
    private decimal _debit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineAmount))]
    private decimal _credit;

    public decimal LineAmount => Debit > 0 ? Debit : Credit;
}

public partial class JournalEntryModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private string _referenceNumber = $"JV-{DateTime.UtcNow:MMddHHmm}";

    [ObservableProperty]
    private string _description = "Monthly general ledger adjustment entry";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<JournalEntryLineViewModel> JournalLines { get; } = new();

    public decimal TotalDebit => JournalLines.Sum(l => l.Debit);
    public decimal TotalCredit => JournalLines.Sum(l => l.Credit);
    public decimal UnbalancedDifference => Math.Abs(TotalDebit - TotalCredit);
    public bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;
    public bool IsUnbalanced => TotalDebit != TotalCredit;

    public JournalEntryModalViewModel(
        IMediator mediator,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        AddSampleLines();
    }

    private void AddSampleLines()
    {
        var line1 = new JournalEntryLineViewModel
        {
            AccountCode = "1010",
            AccountName = "1010 - Operating Cash Account",
            Description = "Received client invoice payment",
            Debit = 5000.00m,
            Credit = 0m
        };

        var line2 = new JournalEntryLineViewModel
        {
            AccountCode = "4010",
            AccountName = "4010 - Sales Revenue",
            Description = "Recognize consulting service revenue",
            Debit = 0m,
            Credit = 5000.00m
        };

        line1.PropertyChanged += (s, e) => RecalculateTotals();
        line2.PropertyChanged += (s, e) => RecalculateTotals();

        JournalLines.Add(line1);
        JournalLines.Add(line2);

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(TotalDebit));
        OnPropertyChanged(nameof(TotalCredit));
        OnPropertyChanged(nameof(UnbalancedDifference));
        OnPropertyChanged(nameof(IsBalanced));
        OnPropertyChanged(nameof(IsUnbalanced));
    }

    [RelayCommand]
    public void AddLine()
    {
        var newLine = new JournalEntryLineViewModel
        {
            AccountCode = "5010",
            AccountName = "5010 - General Expense",
            Description = "Adjustment entry",
            Debit = 0m,
            Credit = 0m
        };

        newLine.PropertyChanged += (s, e) => RecalculateTotals();
        JournalLines.Add(newLine);
        RecalculateTotals();
    }

    [RelayCommand]
    public void RemoveLine(JournalEntryLineViewModel line)
    {
        if (JournalLines.Contains(line))
        {
            JournalLines.Remove(line);
            RecalculateTotals();
        }
    }

    [RelayCommand]
    public async Task PostJournalAsync()
    {
        if (!IsBalanced)
        {
            StatusMessage = $"Cannot post unbalanced journal! Difference: ${UnbalancedDifference:N2}";
            return;
        }

        IsBusy = true;
        StatusMessage = "Posting balanced journal entry to General Ledger...";

        try
        {
            var lineDtos = JournalLines.Select(l => new JournalLineDto(l.AccountCode, l.AccountName, l.Description, l.Debit, l.Credit)).ToList();
            var command = new PostJournalEntryCommand(ReferenceNumber, Description, lineDtos);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Posted Journal Voucher #{ReferenceNumber} cleanly!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Posting failed: {result.Error.Description}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCloseRequested?.Invoke();
    }
}
