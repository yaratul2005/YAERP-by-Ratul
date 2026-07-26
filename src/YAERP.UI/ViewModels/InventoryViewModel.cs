using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Inventory.Commands.ExportInventoryExcel;

namespace YAERP.UI.ViewModels;

public record ProductDto(string Name, string SKU, decimal Price, decimal Stock);

public partial class InventoryViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _products = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    public InventoryViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        // Mock query flow for UI loading
        await Task.Delay(200);
        Products.Clear();
        Products.Add(new ProductDto("Sample Item A", "SKU-001", 19.99m, 150m));
        Products.Add(new ProductDto("Sample Item B", "SKU-002", 49.99m, 20m));
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
}
