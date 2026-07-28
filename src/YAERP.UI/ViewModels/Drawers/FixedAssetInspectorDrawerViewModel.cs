using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Financials.Commands.PostDepreciationRun;

namespace YAERP.UI.ViewModels.Drawers;

public record DepreciationScheduleLineDto(int Year, string Period, decimal BeginningBookValue, decimal DepreciationExpense, decimal EndingBookValue);

public partial class FixedAssetInspectorDrawerViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private Guid _assetId;

    [ObservableProperty]
    private string _assetTag = "AST-CNC-001";

    [ObservableProperty]
    private string _assetName = "CNC Milling Machine Alpha";

    [ObservableProperty]
    private string _category = "Manufacturing Machinery";

    [ObservableProperty]
    private decimal _acquisitionCost = 150000m;

    [ObservableProperty]
    private decimal _salvageValue = 15000m;

    [ObservableProperty]
    private int _usefulLifeYears = 10;

    [ObservableProperty]
    private decimal _accumulatedDepreciation = 45000m;

    [ObservableProperty]
    private string _selectedDepreciationMethod = "Straight-Line";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public decimal BookValue => Math.Max(SalvageValue, AcquisitionCost - AccumulatedDepreciation);
    public decimal LifeConsumedPercent => AcquisitionCost > 0 ? (AccumulatedDepreciation / AcquisitionCost) * 100m : 0m;

    public ObservableCollection<DepreciationScheduleLineDto> ScheduleLines { get; } = new();

    public FixedAssetInspectorDrawerViewModel(
        IMediator mediator,
        Guid assetId = default,
        string assetTag = "AST-001",
        string assetName = "CNC Machine Alpha",
        decimal acquisitionCost = 150000m,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _assetId = assetId != default ? assetId : Guid.NewGuid();
        _assetTag = assetTag;
        _assetName = assetName;
        _acquisitionCost = acquisitionCost;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        CalculateDepreciationSchedule();
    }

    private void CalculateDepreciationSchedule()
    {
        ScheduleLines.Clear();
        decimal depreciableBase = AcquisitionCost - SalvageValue;
        decimal annualDepreciation = UsefulLifeYears > 0 ? depreciableBase / UsefulLifeYears : 0m;

        decimal currentBookValue = AcquisitionCost;

        for (int yr = 1; yr <= 5; yr++)
        {
            decimal expense = annualDepreciation;
            decimal endingBook = Math.Max(SalvageValue, currentBookValue - expense);

            ScheduleLines.Add(new DepreciationScheduleLineDto(yr, $"Year {yr}", currentBookValue, expense, endingBook));
            currentBookValue = endingBook;
        }
    }

    [RelayCommand]
    public async Task ExecuteDepreciationRunAsync()
    {
        IsBusy = true;
        StatusMessage = "Processing asset depreciation posting...";

        try
        {
            var command = new PostDepreciationRunCommand(
                AssetId,
                $"{DateTime.UtcNow:yyyy}-M{DateTime.UtcNow:MM}",
                ScheduleLines.Count > 0 ? ScheduleLines[0].DepreciationExpense : 2500m,
                SelectedDepreciationMethod);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Posted depreciation run for {AssetTag} ({AssetName})!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"Depreciation posting failed: {result.Error.Description}";
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
