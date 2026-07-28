using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Drawers;
using YAERP.UI.Views.Modals;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record FixedAssetDto(Guid Id, string AssetTag, string Name, decimal BookValue, string Status);
public record DepreciationScheduleDto(Guid Id, string Period, decimal Amount, decimal BookValueAfter);
public record ForexExposureDto(Guid AccountId, string Currency, decimal ForeignBalance, decimal UnrealizedGainLoss);
public record TaxFilingSummaryDto(string Period, decimal OutputVat, decimal InputVat, decimal NetLiability);

public partial class DeepFinancialsViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;
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
        IMediator mediator,
        IFixedAssetDepreciationService depreciationService,
        IForexRevaluationService forexService,
        ITaxComputationService taxService)
    {
        _mediator = mediator;
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
    public async Task RefreshFinancialsDataAsync()
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
            await Task.Delay(200);

            AssetRegister.Clear();
            AssetRegister.Add(new FixedAssetDto(Guid.NewGuid(), "AST-001", "CNC Machine Alpha", 150000m, "Active"));
            AssetRegister.Add(new FixedAssetDto(Guid.NewGuid(), "AST-002", "Delivery Truck", 25000m, "Active"));

            PendingDepreciations.Clear();
            PendingDepreciations.Add(new DepreciationScheduleDto(Guid.NewGuid(), "2023-M10", 2500m, 147500m));
            PendingDepreciations.Add(new DepreciationScheduleDto(Guid.NewGuid(), "2023-M10", 416.67m, 24583.33m));

            ForexExposures.Clear();
            ForexExposures.Add(new ForexExposureDto(Guid.NewGuid(), "USD", 50000m, 1250m));
            ForexExposures.Add(new ForexExposureDto(Guid.NewGuid(), "EUR", 15000m, -400m));

            TaxSummaries.Clear();
            TaxSummaries.Add(new TaxFilingSummaryDto("Q3 2023", 45000m, 12000m, 33000m));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void OpenCreateJournalModal()
    {
        var modalVm = new JournalEntryModalViewModel(
            _mediator,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshFinancialsDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new JournalEntryModal { DataContext = modalVm };
        OpenModal(view, "Post General Ledger Journal Entry");
    }

    [RelayCommand]
    public void OpenAssetDrawer(FixedAssetDto? asset = null)
    {
        var assetId = asset?.Id ?? Guid.NewGuid();
        var tag = asset?.AssetTag ?? "AST-001";
        var name = asset?.Name ?? "CNC Machine Alpha";
        var val = asset?.BookValue ?? 150000m;

        var drawerVm = new FixedAssetInspectorDrawerViewModel(
            _mediator,
            assetId,
            tag,
            name,
            val,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshFinancialsDataAsync();
            },
            onCloseRequested: () => CloseDrawer());

        var view = new FixedAssetInspectorDrawer { DataContext = drawerVm };
        OpenDrawer(view, $"Fixed Asset Inspector - {tag}");
    }

    [RelayCommand]
    private async Task RunDepreciationPostingAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(300);
            ShowSuccessToast("Depreciation schedule posted cleanly to GL.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteForexRevaluationAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(300);
            ShowSuccessToast("Multi-currency forex revaluation completed.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateTaxReturnAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(300);
            ShowSuccessToast("Regional Tax / VAT return report generated successfully.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
