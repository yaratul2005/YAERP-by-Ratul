using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace YAERP.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _totalSales;

    [ObservableProperty]
    private int _stockCount;

    [ObservableProperty]
    private int _activeInvoices;

    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoadMetricsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            // Simulate fetching from mediator queries
            await Task.Delay(500);
            TotalSales = 120;
            StockCount = 4500;
            ActiveInvoices = 15;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
