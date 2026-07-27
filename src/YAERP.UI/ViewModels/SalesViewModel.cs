using System.Collections.ObjectModel;
using YAERP.UI.Workspace;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace YAERP.UI.ViewModels;

public record SalesOrderDto(string OrderNumber, string CustomerName, decimal TotalAmount, string Status);

public partial class SalesViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private ObservableCollection<SalesOrderDto> _salesOrders = new();

    public SalesViewModel(IMediator mediator)
    {
        Title = "Sales";
        IconKey = "🛒";
        TabId = "SalesViewModel";
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadSalesOrdersAsync()
    {
        await Task.Delay(200);
        SalesOrders.Clear();
        SalesOrders.Add(new SalesOrderDto("SO-1001", "Acme Corp", 1500.00m, "Draft"));
    }

    [RelayCommand]
    private async Task CreateSalesOrderAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task FulfillOrderAsync(SalesOrderDto order)
    {
        await Task.CompletedTask;
    }
}
