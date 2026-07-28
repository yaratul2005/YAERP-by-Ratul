using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Sales.Commands.UpdateCustomerProfile;

namespace YAERP.UI.ViewModels.Modals;

public record CustomerOrderHistoryDto(string OrderNumber, DateTime OrderDate, decimal TotalAmount, string PaymentStatus);
public record CustomerActivityLogDto(DateTime Timestamp, string Author, string ActionSummary);

public partial class Customer360ModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private Guid _customerId;

    [ObservableProperty]
    private string _customerName = "Acme Global Solutions";

    [ObservableProperty]
    private string _taxId = "US-9948201";

    [ObservableProperty]
    private string _email = "billing@acmeglobal.com";

    [ObservableProperty]
    private string _phone = "+1 (555) 019-2834";

    [ObservableProperty]
    private string _shippingAddress = "742 Evergreen Terrace, Sector 4, Springfield";

    [ObservableProperty]
    private decimal _creditLimit = 50000m;

    [ObservableProperty]
    private decimal _outstandingArBalance = 12500m;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public decimal CreditUsagePercent => CreditLimit > 0 ? (OutstandingArBalance / CreditLimit) * 100m : 0m;
    public decimal AvailableCreditLine => Math.Max(0, CreditLimit - OutstandingArBalance);

    public ObservableCollection<CustomerOrderHistoryDto> OrderHistory { get; } = new();
    public ObservableCollection<CustomerActivityLogDto> ActivityLogs { get; } = new();

    public Customer360ModalViewModel(
        IMediator mediator,
        Guid customerId,
        string customerName,
        decimal creditLimit = 50000m,
        decimal outstandingArBalance = 12500m,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _customerId = customerId;
        _customerName = customerName;
        _creditLimit = creditLimit;
        _outstandingArBalance = outstandingArBalance;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        LoadData();
    }

    private void LoadData()
    {
        OrderHistory.Clear();
        OrderHistory.Add(new CustomerOrderHistoryDto("SO-10088", DateTime.UtcNow.AddDays(-15), 8400.00m, "Paid"));
        OrderHistory.Add(new CustomerOrderHistoryDto("SO-10094", DateTime.UtcNow.AddDays(-3), 4100.00m, "Pending AR"));

        ActivityLogs.Clear();
        ActivityLogs.Add(new CustomerActivityLogDto(DateTime.UtcNow.AddDays(-2), "Sales Rep John", "Sent quarterly renewal quotation via email."));
        ActivityLogs.Add(new CustomerActivityLogDto(DateTime.UtcNow.AddDays(-10), "Finance Dept", "Credit line reviewed and increased to $50,000."));
    }

    [RelayCommand]
    public async Task SaveCustomerDetailsAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            StatusMessage = "Customer Name is required.";
            return;
        }

        if (CreditLimit < 0)
        {
            StatusMessage = "Credit limit cannot be negative.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Updating customer 360° master record...";

        try
        {
            var command = new UpdateCustomerProfileCommand(
                CustomerId,
                CustomerName,
                Email,
                Phone,
                ShippingAddress,
                CreditLimit);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Updated 360° profile for {CustomerName}!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Update failed: {result.Error.Description}";
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
