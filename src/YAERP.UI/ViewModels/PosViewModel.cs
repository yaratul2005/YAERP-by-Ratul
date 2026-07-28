using System;
using YAERP.UI.Workspace;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Sales.DTOs;
using YAERP.Infrastructure.Hardware;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Modals;

namespace YAERP.UI.ViewModels;

public record CartItemDto(
    Guid ProductId,
    string SKU,
    string Name,
    decimal Quantity,
    decimal UnitPrice)
{
    public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);
}

public partial class PosViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IReceiptPrinterService _receiptPrinterService;
    private readonly BarcodeScannerListener _barcodeListener;

    [ObservableProperty]
    private ObservableCollection<CartItemDto> _currentCart = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> _catalogProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedCatalogProduct;

    [ObservableProperty]
    private string _searchBarcodeQuery = string.Empty;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private string _selectedPaymentMethod = "Cash";

    [ObservableProperty]
    private decimal _cashTendered = 100.00m;

    [ObservableProperty]
    private decimal _changeDue;

    [ObservableProperty]
    private bool _autoPrintReceipt = true;

    [ObservableProperty]
    private bool _autoOpenCashDrawer = true;

    [ObservableProperty]
    private string _selectedPrinterName = "MOCK";

    [ObservableProperty]
    private string _selectedCategoryFilter = "All";

    [ObservableProperty]
    private bool _isProcessingCheckout;

    [ObservableProperty]
    private string? _statusMessage;

    public PosViewModel(
        IMediator mediator,
        IReceiptPrinterService receiptPrinterService,
        BarcodeScannerListener barcodeListener)
    {
        Title = "POS Terminal";
        IconKey = "🖥️";
        TabId = "PosViewModel";
        _mediator = mediator;
        _receiptPrinterService = receiptPrinterService;
        _barcodeListener = barcodeListener;

        // Wire barcode scanner listener event
        _barcodeListener.BarcodeScanned += OnBarcodeScanned;

        // Load sample catalog items
        LoadCatalogProducts();
    }

    private void LoadCatalogProducts()
    {
        CatalogProducts.Clear();
        CatalogProducts.Add(new ProductDto(Guid.NewGuid(), "Wireless Laser Mouse", "SKU-1001", 25.00m, 150m));
        CatalogProducts.Add(new ProductDto(Guid.NewGuid(), "RGB Mechanical Keyboard", "SKU-1002", 89.99m, 45m));
        CatalogProducts.Add(new ProductDto(Guid.NewGuid(), "27-inch 4K Monitor", "SKU-1003", 349.50m, 18m));
        CatalogProducts.Add(new ProductDto(Guid.NewGuid(), "USB-C Fast Charger", "SKU-1004", 19.99m, 200m));
        CatalogProducts.Add(new ProductDto(Guid.NewGuid(), "Ergonomic Desk Chair", "SKU-1005", 220.00m, 12m));
    }

    private void OnBarcodeScanned(object? sender, string barcode)
    {
        SearchBarcodeQuery = barcode;
        var matchedProduct = CatalogProducts.FirstOrDefault(p =>
            p.SKU.Equals(barcode, StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains(barcode, StringComparison.OrdinalIgnoreCase));

        if (matchedProduct != null)
        {
            AddToCart(matchedProduct);
            StatusMessage = $"⚡ Barcode Scan Hit: Added '{matchedProduct.Name}' to cart.";
        }
        else
        {
            StatusMessage = $"⚠️ Unknown Barcode: '{barcode}'. SKU not found in local catalog.";
        }
    }

    [RelayCommand]
    public void AddToCart(ProductDto product)
    {
        if (product == null) return;

        var existingItem = CurrentCart.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingItem != null)
        {
            int index = CurrentCart.IndexOf(existingItem);
            CurrentCart[index] = existingItem with { Quantity = existingItem.Quantity + 1 };
        }
        else
        {
            CurrentCart.Add(new CartItemDto(product.Id, product.SKU, product.Name, 1, product.Price));
        }

        RecalculateTotals();
    }

    [RelayCommand]
    public void RemoveFromCart(CartItemDto item)
    {
        if (item == null) return;
        CurrentCart.Remove(item);
        RecalculateTotals();
    }

    [RelayCommand]
    public void IncreaseQuantity(CartItemDto item)
    {
        if (item == null) return;
        int index = CurrentCart.IndexOf(item);
        if (index >= 0)
        {
            CurrentCart[index] = item with { Quantity = item.Quantity + 1 };
            RecalculateTotals();
        }
    }

    [RelayCommand]
    public void DecreaseQuantity(CartItemDto item)
    {
        if (item == null) return;
        int index = CurrentCart.IndexOf(item);
        if (index >= 0)
        {
            if (item.Quantity > 1)
            {
                CurrentCart[index] = item with { Quantity = item.Quantity - 1 };
            }
            else
            {
                CurrentCart.RemoveAt(index);
            }
            RecalculateTotals();
        }
    }

    [RelayCommand]
    public void ClearCart()
    {
        CurrentCart.Clear();
        RecalculateTotals();
        StatusMessage = "Cart cleared.";
    }

    partial void OnCashTenderedChanged(decimal value) => RecalculateTotals();
    partial void OnDiscountAmountChanged(decimal value) => RecalculateTotals();
    partial void OnSelectedPaymentMethodChanged(string value) => RecalculateTotals();

    private void RecalculateTotals()
    {
        SubTotal = CurrentCart.Sum(x => x.LineTotal);
        TaxAmount = Math.Round(SubTotal * 0.10m, 2); // 10% tax rate
        GrandTotal = Math.Max(0, SubTotal + TaxAmount - DiscountAmount);
        ChangeDue = SelectedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase)
            ? Math.Max(0, CashTendered - GrandTotal)
            : 0;
    }

    [RelayCommand]
    public void OpenCashTenderModal()
    {
        if (CurrentCart.Count == 0)
        {
            ShowErrorToast("Shopping cart is empty. Add products to cart first.");
            return;
        }

        var modalVm = new CashTenderModalViewModel(
            _mediator,
            _receiptPrinterService,
            GrandTotal,
            SubTotal,
            TaxAmount,
            CurrentCart.ToList(),
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                ClearCart();
            },
            onCloseRequested: () => CloseModal());

        var view = new CashTenderModal { DataContext = modalVm };
        OpenModal(view, "Cash Payment & Keypad Tender");
    }

    [RelayCommand]
    private async Task CompleteTransactionAsync()
    {
        if (CurrentCart.Count == 0 || IsProcessingCheckout) return;

        IsProcessingCheckout = true;
        StatusMessage = "Processing transaction & hardware interop...";

        try
        {
            string receiptNumber = $"POS-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var lineItems = CurrentCart.Select(c => new SalesReceiptItemDto(
                ItemName: c.Name,
                Quantity: c.Quantity,
                UnitPrice: c.UnitPrice,
                LineTotal: c.LineTotal)).ToList();

            var receiptDto = new SalesReceiptDto(
                ReceiptNumber: receiptNumber,
                TransactionDate: DateTime.UtcNow,
                CashierName: "System Operator",
                CustomerName: "Walk-in Retail Customer",
                LineItems: lineItems,
                SubTotal: SubTotal,
                TaxAmount: TaxAmount,
                TotalAmount: GrandTotal,
                PaymentMethod: SelectedPaymentMethod);

            // Hardware Interop Step 1: Print Thermal ESC/POS Receipt
            if (AutoPrintReceipt)
            {
                await _receiptPrinterService.PrintSalesReceiptAsync(receiptDto, SelectedPrinterName);
            }

            // Hardware Interop Step 2: Open Cash Drawer Kick Pulse (if Cash payment)
            if (AutoOpenCashDrawer && SelectedPaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                await _receiptPrinterService.OpenCashDrawerAsync(SelectedPrinterName);
            }

            StatusMessage = $"✅ Checkout Complete! Receipt #{receiptNumber} generated. Change due: ${ChangeDue:F2}";
            CurrentCart.Clear();
            RecalculateTotals();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Transaction Error: {ex.Message}";
        }
        finally
        {
            IsProcessingCheckout = false;
        }
    }
}
