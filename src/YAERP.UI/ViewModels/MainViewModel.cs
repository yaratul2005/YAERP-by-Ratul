using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.UI.Services;

namespace YAERP.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public ObservableObject? CurrentView => _navigationService.CurrentView;

    [RelayCommand]
    private void NavigateDashboard() => _navigationService.NavigateTo<DashboardViewModel>();

    [RelayCommand]
    private void NavigateInventory() => _navigationService.NavigateTo<InventoryViewModel>();

    [RelayCommand]
    private void NavigateSales() => _navigationService.NavigateTo<SalesViewModel>();

    [RelayCommand]
    private void NavigateFinance() => _navigationService.NavigateTo<FinanceViewModel>();
}
