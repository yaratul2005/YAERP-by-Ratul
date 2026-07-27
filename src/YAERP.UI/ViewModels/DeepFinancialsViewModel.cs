using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record FixedAssetDto(Guid Id, string AssetTag, string Name, decimal BookValue, string Status);
public record DepreciationScheduleDto(Guid Id, string Period, decimal Amount, decimal BookValueAfter);
public record ForexExposureDto(Guid AccountId, string Currency, decimal ForeignBalance, decimal UnrealizedGainLoss);
public record TaxFilingSummaryDto(string Period, decimal OutputVat, decimal InputVat, decimal NetLiability);

public partial class DeepFinancialsViewModel : TabViewModelBase
{
    private readonly IFixedAssetDepreciationService _depreciationService;
    private readonly IForexRevaluationService _forexService;
    private readonly ITaxComputationService _taxService;

    public ObservableCollection<FixedAssetDto> AssetRegister { get; } = new();
    public ObservableCollection<DepreciationScheduleDto> PendingDepreciations { get; } = new();
    public ObservableCollection<ForexExposureDto> ForexExposures { get; } = new();
    public ObservableCollection<TaxFilingSummaryDto> TaxSummaries { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public DeepFinancialsViewModel(
        IFixedAssetDepreciationService depreciationService,
        IForexRevaluationService forexService,
        ITaxComputationService taxService)
    {
        _depreciationService = depreciationService;
        _forexService = forexService;
        _taxService = taxService;

        Title = "Deep Financials & Tax";
        IconKey = "📈";
        TabId = "DeepFinancialsViewModel";
    }

    public override async Task OnTabActivatedAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await Task.Delay(300); // Simulate network load

            // Mock Assets
            AssetRegister.Clear();
            AssetRegister.Add(new FixedAssetDto(Guid.NewGuid(), "AST-001", "CNC Machine Alpha", 150000m, "Active"));
            AssetRegister.Add(new FixedAssetDto(Guid.NewGuid(), "AST-002", "Delivery Truck", 25000m, "Active"));

            // Mock Depreciations
            PendingDepreciations.Clear();
            PendingDepreciations.Add(new DepreciationScheduleDto(Guid.NewGuid(), "2023-M10", 2500m, 147500m));
            PendingDepreciations.Add(new DepreciationScheduleDto(Guid.NewGuid(), "2023-M10", 416.67m, 24583.33m));

            // Mock Forex
            ForexExposures.Clear();
            ForexExposures.Add(new ForexExposureDto(Guid.NewGuid(), "USD", 50000m, 1250m));
            ForexExposures.Add(new ForexExposureDto(Guid.NewGuid(), "EUR", 15000m, -400m));

            // Mock Tax
            TaxSummaries.Clear();
            TaxSummaries.Add(new TaxFilingSummaryDto("Q3 2023", 45000m, 12000m, 33000m));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunDepreciationPostingAsync()
    {
        // Mock execution
        IsBusy = true;
        await Task.Delay(500);
        PendingDepreciations.Clear();
        IsBusy = false;
    }

    [RelayCommand]
    private async Task ExecuteForexRevaluationAsync()
    {
        IsBusy = true;
        await Task.Delay(500);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task GenerateTaxReturnAsync()
    {
        IsBusy = true;
        await Task.Delay(500);
        IsBusy = false;
    }
}
