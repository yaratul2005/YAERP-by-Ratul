using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Manufacturing.Commands.CreateBomHeader;

namespace YAERP.UI.ViewModels.Modals;

public partial class BomComponentLineViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _componentProductId = Guid.NewGuid();

    [ObservableProperty]
    private string _componentName = "Raw Aluminum Housing";

    [ObservableProperty]
    private decimal _quantityPerAssembly = 1m;

    [ObservableProperty]
    private decimal _yieldPercent = 98.5m;

    [ObservableProperty]
    private decimal _scrapFactorPercent = 1.5m;
}

public partial class BomEditorModalViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly Action<string>? _onSuccessNotification;
    private readonly Action? _onCloseRequested;

    [ObservableProperty]
    private string _assemblyName = "Gaming PC Tower (Ultra)";

    [ObservableProperty]
    private string _revisionCode = "REV-B2";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<BomComponentLineViewModel> ComponentLines { get; } = new();

    public BomEditorModalViewModel(
        IMediator mediator,
        Action<string>? onSuccessNotification = null,
        Action? onCloseRequested = null)
    {
        _mediator = mediator;
        _onSuccessNotification = onSuccessNotification;
        _onCloseRequested = onCloseRequested;

        AddSampleComponents();
    }

    private void AddSampleComponents()
    {
        ComponentLines.Clear();
        ComponentLines.Add(new BomComponentLineViewModel { ComponentName = "Intel Core i9 14900K Processor", QuantityPerAssembly = 1m, YieldPercent = 100m, ScrapFactorPercent = 0m });
        ComponentLines.Add(new BomComponentLineViewModel { ComponentName = "NVIDIA RTX 4090 GPU Card", QuantityPerAssembly = 1m, YieldPercent = 99m, ScrapFactorPercent = 1m });
        ComponentLines.Add(new BomComponentLineViewModel { ComponentName = "DDR5 RAM 64GB Module", QuantityPerAssembly = 2m, YieldPercent = 100m, ScrapFactorPercent = 0m });
    }

    [RelayCommand]
    public void AddComponent()
    {
        ComponentLines.Add(new BomComponentLineViewModel
        {
            ComponentProductId = Guid.NewGuid(),
            ComponentName = "New Component SKU",
            QuantityPerAssembly = 1m,
            YieldPercent = 100m,
            ScrapFactorPercent = 0m
        });
    }

    [RelayCommand]
    public void RemoveComponent(BomComponentLineViewModel line)
    {
        if (ComponentLines.Contains(line))
        {
            ComponentLines.Remove(line);
        }
    }

    [RelayCommand]
    public async Task SaveBomAsync()
    {
        if (string.IsNullOrWhiteSpace(AssemblyName))
        {
            StatusMessage = "Assembly Name is required.";
            return;
        }

        if (ComponentLines.Count == 0)
        {
            StatusMessage = "BOM must contain at least one component.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Validating DAG hierarchy & saving Bill of Materials...";

        try
        {
            var lineDtos = ComponentLines.Select(c => new BomComponentDto(c.ComponentProductId, c.QuantityPerAssembly, c.YieldPercent, c.ScrapFactorPercent)).ToList();
            var command = new CreateBomHeaderCommand(AssemblyName, RevisionCode, lineDtos);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _onSuccessNotification?.Invoke($"Created BOM Header '{AssemblyName}' ({RevisionCode}) with {lineDtos.Count} components!");
                _onCloseRequested?.Invoke();
            }
            else
            {
                StatusMessage = $"BOM Save Failed: {result.Error.Description}";
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
