using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Sales.Commands.ProcessPosSale;
using YAERP.Application.Sales.DTOs;
using YAERP.UI.ViewModels;

namespace YAERP.UI.ViewModels.Modals;

public partial class CashTenderModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly IReceiptPrinterService _receiptPrinterService;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;
    private readonly List<CartItemDto> _cartItems;
    private readonly decimal _subTotal;
    private readonly decimal _taxAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChangeDue))]
    [NotifyPropertyChangedFor(nameof(IsSufficient))]
    private decimal _grandTotal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChangeDue))]
    [NotifyPropertyChangedFor(nameof(IsSufficient))]
    private decimal _cashTendered;

    [ObservableProperty]
    private string _keypadInputText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public decimal ChangeDue => Math.Max(0, CashTendered - GrandTotal);
    public bool IsSufficient => CashTendered >= GrandTotal && GrandTotal > 0;

    public CashTenderModalViewModel(
        IMediator mediator,
        IReceiptPrinterService receiptPrinterService,
        decimal grandTotal,
        decimal subTotal,
        decimal taxAmount,
        List<CartItemDto> cartItems,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _receiptPrinterService = receiptPrinterService;
        _grandTotal = grandTotal;
        _subTotal = subTotal;
        _taxAmount = taxAmount;
        _cartItems = cartItems;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        // Default tendered amount to exact or next $20 bill
        _cashTendered = Math.Ceiling(grandTotal / 20m) * 20m;
        if (_cashTendered < grandTotal) _cashTendered = grandTotal;
        _keypadInputText = _cashTendered.ToString("F2");
    }

    [RelayCommand]
    public void AppendDigit(string digit)
    {
        if (digit == "C")
        {
            KeypadInputText = "0";
            CashTendered = 0m;
            return;
        }

        if (KeypadInputText == "0" && digit != ".")
        {
            KeypadInputText = digit;
        }
        else
        {
            KeypadInputText += digit;
        }

        if (decimal.TryParse(KeypadInputText, out var val))
        {
            CashTendered = val;
        }
    }

    [RelayCommand]
    public void SetPresetAmount(string amountStr)
    {
        if (amountStr == "Exact")
        {
            CashTendered = GrandTotal;
        }
        else if (decimal.TryParse(amountStr, out var preset))
        {
            CashTendered = preset;
        }

        KeypadInputText = CashTendered.ToString("F2");
    }

    [RelayCommand]
    public async Task CompleteSaleAsync()
    {
        if (!IsSufficient)
        {
            StatusMessage = $"Insufficient cash tendered! Short by ${GrandTotal - CashTendered:F2}";
            return;
        }

        IsBusy = true;
        StatusMessage = "Processing POS transaction & hardware receipt print...";

        try
        {
            string receiptNumber = $"POS-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var lineDtos = _cartItems.Select(c => new PosCartLineDto(c.ProductId, c.SKU, c.Name, c.Quantity, c.UnitPrice)).ToList();

            var command = new ProcessPosSaleCommand(
                receiptNumber,
                _subTotal,
                _taxAmount,
                GrandTotal,
                "Cash",
                CashTendered,
                ChangeDue,
                lineDtos);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                // ESC/POS Receipt Printing & Cash Drawer Pulse via Hardware Interop
                var receiptItemDtos = _cartItems.Select(c => new SalesReceiptItemDto(c.Name, c.Quantity, c.UnitPrice, c.LineTotal)).ToList();
                var receiptDto = new SalesReceiptDto(
                    receiptNumber,
                    DateTime.UtcNow,
                    "System Operator",
                    "Walk-in Retail Customer",
                    receiptItemDtos,
                    _subTotal,
                    _taxAmount,
                    GrandTotal,
                    "Cash");

                await _receiptPrinterService.PrintSalesReceiptAsync(receiptDto, "MOCK");
                await _receiptPrinterService.OpenCashDrawerAsync("MOCK");

                _onSuccessNotification?.Invoke($"Sale Completed! Receipt #{receiptNumber}. Change due: ${ChangeDue:F2}");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Sale failed: {result.Error.Description}";
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
