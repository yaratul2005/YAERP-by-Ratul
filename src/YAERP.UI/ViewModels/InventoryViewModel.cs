using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Inventory.Commands.ExportInventoryExcel;
using YAERP.Application.Inventory.Queries.GetProductDemandForecast;

namespace YAERP.UI.ViewModels;

public record ProductDto(Guid Id, string Name, string SKU, decimal Price, decimal Stock);

public partial class InventoryViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isAiAnalyzing;

    [ObservableProperty]
    private decimal _predictedDemand30Days;

    [ObservableProperty]
    private string? _stockoutWarningMessage;

    public InventoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        await Task.Delay(200);
        Products.Clear();
        Products.Add(new ProductDto(Guid.NewGuid(), "Sample Item A", "SKU-001", 19.99m, 150m));
        Products.Add(new ProductDto(Guid.NewGuid(), "Sample Item B", "SKU-002", 49.99m, 20m));
    }

    [RelayCommand]
    private async Task CreateProductAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        var result = await _mediator.Send(new ExportInventoryExcelCommand());
        if (result.IsSuccess)
        {
            await System.IO.File.WriteAllBytesAsync("InventoryStock.xlsx", result.Value);
        }
    }

    [RelayCommand]
    private async Task LoadAiForecastAsync()
    {
        if (SelectedProduct == null || IsAiAnalyzing) return;

        IsAiAnalyzing = true;
        StockoutWarningMessage = null;

        try
        {
            var command = new GetProductDemandForecastQuery(SelectedProduct.Id, Guid.NewGuid(), 30);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                decimal total = 0;
                foreach (var qty in result.Value.ForecastedDemand30Days)
                {
                    total += qty;
                }

                PredictedDemand30Days = total;

                if (result.Value.StockoutWarning)
                {
                    StockoutWarningMessage = "Warning: High risk of stockout within 14 days based on predicted consumption!";
                }
            }
        }
        finally
        {
            IsAiAnalyzing = false;
        }
    }
}
