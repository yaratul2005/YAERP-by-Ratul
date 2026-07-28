using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using YAERP.Application.Sales.Commands.UpdateDealStage;
using YAERP.UI.ViewModels.Modals;
using YAERP.UI.Views.Modals;
using YAERP.UI.Workspace;

namespace YAERP.UI.ViewModels;

public record SalesDealDto(Guid DealId, string DealName, string CustomerName, decimal ExpectedRevenue, int ProbabilityPercent, string Stage);
public record SalesOrderDto(string OrderNumber, string CustomerName, decimal TotalAmount, string Status);
public record CustomerSummaryDto(Guid CustomerId, string Name, string Email, decimal CreditLimit, decimal OutstandingAr);

public partial class SalesViewModel : TabViewModelBase
{
    private readonly IMediator _mediator;

    public ObservableCollection<SalesDealDto> KanbanDeals { get; } = new();
    public ObservableCollection<SalesOrderDto> SalesOrders { get; } = new();
    public ObservableCollection<CustomerSummaryDto> Customers { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _selectedStageFilter = "All";

    public SalesViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Title = "Sales & CRM Pipeline";
        IconKey = "🛒";
        TabId = "SalesViewModel";
    }

    public override async Task OnTabActivatedAsync()
    {
        await RefreshSalesDataAsync();
    }

    [RelayCommand]
    public async Task RefreshSalesDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await Task.Delay(150);

            KanbanDeals.Clear();
            KanbanDeals.Add(new SalesDealDto(Guid.NewGuid(), "Cloud Infrastructure Upgrade", "Acme Global", 45000m, 60, "Qualification"));
            KanbanDeals.Add(new SalesDealDto(Guid.NewGuid(), "POS Terminal Hardware Batch", "Retail Chain X", 82000m, 80, "Proposal"));
            KanbanDeals.Add(new SalesDealDto(Guid.NewGuid(), "Enterprise ERP Licensing", "Stark Industries", 125000m, 90, "Negotiation"));
            KanbanDeals.Add(new SalesDealDto(Guid.NewGuid(), "Supply Chain Consulting", "Wayne Enterprises", 38000m, 100, "Closed Won"));

            SalesOrders.Clear();
            SalesOrders.Add(new SalesOrderDto("SO-10088", "Acme Global Solutions", 8400.00m, "Confirmed"));
            SalesOrders.Add(new SalesOrderDto("SO-10094", "Stark Industries", 4100.00m, "Draft"));

            Customers.Clear();
            Customers.Add(new CustomerSummaryDto(Guid.NewGuid(), "Acme Global Solutions", "billing@acmeglobal.com", 50000m, 12500m));
            Customers.Add(new CustomerSummaryDto(Guid.NewGuid(), "Stark Industries", "finance@stark.com", 150000m, 32000m));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void OpenCreateQuotationModal()
    {
        var modalVm = new SalesOrderBuilderModalViewModel(
            _mediator,
            isQuotationConversion: true,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshSalesDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new SalesOrderBuilderModal { DataContext = modalVm };
        OpenModal(view, "Convert Quotation into Sales Order");
    }

    [RelayCommand]
    public void OpenCreateSalesOrderModal()
    {
        var modalVm = new SalesOrderBuilderModalViewModel(
            _mediator,
            isQuotationConversion: false,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshSalesDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new SalesOrderBuilderModal { DataContext = modalVm };
        OpenModal(view, "Create New Sales Order");
    }

    [RelayCommand]
    public void OpenCustomer360Modal(CustomerSummaryDto? customer)
    {
        var custId = customer?.CustomerId ?? Guid.NewGuid();
        var custName = customer?.Name ?? "Acme Global Solutions";
        var limit = customer?.CreditLimit ?? 50000m;
        var ar = customer?.OutstandingAr ?? 12500m;

        var modalVm = new Customer360ModalViewModel(
            _mediator,
            custId,
            custName,
            limit,
            ar,
            onSuccessNotification: message =>
            {
                ShowSuccessToast(message);
                _ = RefreshSalesDataAsync();
            },
            onCloseRequested: () => CloseModal());

        var view = new Customer360Modal { DataContext = modalVm };
        OpenModal(view, $"Customer 360° Profile - {custName}");
    }

    [RelayCommand]
    public async Task MoveDealStageAsync(SalesDealDto deal)
    {
        if (deal == null) return;

        string nextStage = deal.Stage switch
        {
            "Qualification" => "Proposal",
            "Proposal" => "Negotiation",
            "Negotiation" => "Closed Won",
            _ => "Closed Won"
        };

        var command = new UpdateDealStageCommand(deal.DealId, nextStage);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            int index = KanbanDeals.IndexOf(deal);
            if (index >= 0)
            {
                KanbanDeals[index] = deal with { Stage = nextStage };
            }
            ShowSuccessToast($"Moved '{deal.DealName}' to stage: {nextStage}!");
        }
        else
        {
            ShowErrorToast($"Stage update failed: {result.Error.Description}");
        }
    }
}
