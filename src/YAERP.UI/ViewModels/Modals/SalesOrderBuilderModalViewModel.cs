using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Sales.Commands.ConvertQuoteToSalesOrder;
using YAERP.Application.Sales.Commands.CreateSalesOrder;

namespace YAERP.UI.ViewModels.Modals;

public record CustomerOptionDto(Guid CustomerId, string Name, decimal AvailableCreditLine);

public partial class SalesOrderLineItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _productId = Guid.NewGuid();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(GrossMarginPercent))]
    [NotifyPropertyChangedFor(nameof(IsLowMargin))]
    private string _productName = "Product Item";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(GrossMarginPercent))]
    [NotifyPropertyChangedFor(nameof(IsLowMargin))]
    private decimal _quantity = 1m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(GrossMarginPercent))]
    [NotifyPropertyChangedFor(nameof(IsLowMargin))]
    private decimal _unitPrice = 100m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GrossMarginPercent))]
    [NotifyPropertyChangedFor(nameof(IsLowMargin))]
    private decimal _unitCost = 70m;

    public decimal LineTotal => Quantity * UnitPrice;

    public decimal GrossMarginPercent => UnitPrice > 0 ? ((UnitPrice - UnitCost) / UnitPrice) * 100m : 0m;

    public bool IsLowMargin => GrossMarginPercent < 20.0m;
}

public partial class SalesOrderBuilderModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private string _orderNumber = $"SO-{DateTime.UtcNow:MMddHHmm}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExceedsCreditLimit))]
    private CustomerOptionDto? _selectedCustomer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubTotal))]
    [NotifyPropertyChangedFor(nameof(TaxAmount))]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    [NotifyPropertyChangedFor(nameof(ExceedsCreditLimit))]
    private decimal _taxRatePercent = 8.5m;

    [ObservableProperty]
    private bool _isQuotationConversion;

    [ObservableProperty]
    private Guid _quotationId;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<CustomerOptionDto> Customers { get; } = new();
    public ObservableCollection<SalesOrderLineItemViewModel> OrderItems { get; } = new();

    public decimal SubTotal => OrderItems.Sum(item => item.LineTotal);
    public decimal TaxAmount => SubTotal * (TaxRatePercent / 100m);
    public decimal GrandTotal => SubTotal + TaxAmount;

    public bool ExceedsCreditLimit => SelectedCustomer != null && GrandTotal > SelectedCustomer.AvailableCreditLine;

    public SalesOrderBuilderModalViewModel(
        IMediator mediator,
        bool isQuotationConversion = false,
        Guid quotationId = default,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _isQuotationConversion = isQuotationConversion;
        _quotationId = quotationId;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        LoadCustomers();
        AddSampleItem();
    }

    private void LoadCustomers()
    {
        Customers.Clear();
        Customers.Add(new CustomerOptionDto(Guid.NewGuid(), "Acme Global Solutions", 37500m));
        Customers.Add(new CustomerOptionDto(Guid.NewGuid(), "Stark Industries", 150000m));
        Customers.Add(new CustomerOptionDto(Guid.NewGuid(), "Wayne Enterprises", 5000m));

        SelectedCustomer = Customers.FirstOrDefault();
    }

    private void AddSampleItem()
    {
        var item1 = new SalesOrderLineItemViewModel
        {
            ProductName = "Enterprise Server Rack (42U)",
            Quantity = 2m,
            UnitPrice = 1450.00m,
            UnitCost = 1100.00m
        };

        var item2 = new SalesOrderLineItemViewModel
        {
            ProductName = "Low-Margin Clearance Switch",
            Quantity = 5m,
            UnitPrice = 200.00m,
            UnitCost = 185.00m // Low margin 7.5% -> flags warning!
        };

        item1.PropertyChanged += (s, e) => RecalculateTotals();
        item2.PropertyChanged += (s, e) => RecalculateTotals();

        OrderItems.Add(item1);
        OrderItems.Add(item2);
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(ExceedsCreditLimit));
    }

    [RelayCommand]
    public void AddLineItem()
    {
        var newItem = new SalesOrderLineItemViewModel
        {
            ProductName = "New Custom Item",
            Quantity = 1m,
            UnitPrice = 500.00m,
            UnitCost = 350.00m
        };

        newItem.PropertyChanged += (s, e) => RecalculateTotals();
        OrderItems.Add(newItem);
        RecalculateTotals();
    }

    [RelayCommand]
    public void RemoveLineItem(SalesOrderLineItemViewModel item)
    {
        if (OrderItems.Contains(item))
        {
            OrderItems.Remove(item);
            RecalculateTotals();
        }
    }

    [RelayCommand]
    public async Task SubmitOrderAsync()
    {
        if (SelectedCustomer == null)
        {
            StatusMessage = "Please select a Customer for the sales order.";
            return;
        }

        if (OrderItems.Count == 0)
        {
            StatusMessage = "Order must contain at least one line item.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Processing sales order & posting to ledger...";

        try
        {
            var lineDtos = OrderItems.Select(i => new SalesOrderItemDto(i.ProductId, i.Quantity, i.UnitPrice)).ToList();

            if (IsQuotationConversion)
            {
                var convertCmd = new ConvertQuoteToSalesOrderCommand(
                    QuotationId,
                    SelectedCustomer.CustomerId,
                    Guid.NewGuid(),
                    OrderNumber,
                    lineDtos);

                var convertResult = await _mediator.Send(convertCmd);

                if (convertResult.IsSuccess)
                {
                    _onSuccessNotification?.Invoke($"Converted Quotation into Sales Order #{OrderNumber}!");
                    _onCloseRequested?.Invoke();
                }
                else
                {
                    StatusMessage = $"Conversion failed: {convertResult.Error.Description}";
                }
            }
            else
            {
                var createCmd = new CreateSalesOrderCommand(
                    SelectedCustomer.CustomerId,
                    Guid.NewGuid(),
                    OrderNumber,
                    lineDtos);

                var createResult = await _mediator.Send(createCmd);

                if (createResult.IsSuccess)
                {
                    _onSuccessNotification?.Invoke($"Created Sales Order #{OrderNumber} successfully!");
                    _onCloseRequested?.Invoke();
                }
                else
                {
                    StatusMessage = $"Order creation failed: {createResult.Error.Description}";
                }
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
