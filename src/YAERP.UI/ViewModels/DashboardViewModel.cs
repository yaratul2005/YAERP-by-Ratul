using System.Threading.Tasks;
using YAERP.UI.Workspace;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.UI.Services;

namespace YAERP.UI.ViewModels;

public partial class DashboardViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
    private readonly INavigationService? _navigationService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _totalSales;

    [ObservableProperty]
    private int _stockCount;

    [ObservableProperty]
    private int _activeInvoices;

    [ObservableProperty]
    private int _aiStockoutRiskCount;

    [ObservableProperty]
    private int _aiLedgerAnomalyCount;

    [ObservableProperty]
    private bool _hasAiAlerts;

    public DashboardViewModel(IMediator mediator, INavigationService? navigationService = null)
    {
        Title = "Dashboard";
        IconKey = "📊";
        TabId = "DashboardViewModel";
        _mediator = mediator;
        _navigationService = navigationService;
    }

    public override async Task OnTabActivatedAsync()
    {
        await LoadMetricsAsync();
    }

    [RelayCommand]
    private async Task LoadMetricsAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            await Task.Delay(200);
            TotalSales = 120;
            StockCount = 4500;
            ActiveInvoices = 15;

            // AI Operational Health summary metrics
            AiStockoutRiskCount = 2;
            AiLedgerAnomalyCount = 1;
            HasAiAlerts = (AiStockoutRiskCount + AiLedgerAnomalyCount) > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NavigateToInventory()
    {
        _navigationService?.NavigateTo<InventoryViewModel>();
    }

    [RelayCommand]
    private void NavigateToFinance()
    {
        _navigationService?.NavigateTo<FinanceViewModel>();
    }
}
