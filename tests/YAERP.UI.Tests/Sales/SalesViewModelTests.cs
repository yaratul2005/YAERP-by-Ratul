using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.Application.Sales.Commands.CreateSalesOrder;
using YAERP.Domain.Common.Primitives;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Modals;

namespace YAERP.UI.Tests.Sales;

public class SalesViewModelTests
{
    [Fact]
    public void SalesOrderBuilder_CalculatesSubtotalTaxAndGrandTotalCorrectly()
    {
        var mockMediator = new Mock<IMediator>();
        var modalVm = new SalesOrderBuilderModalViewModel(mockMediator.Object);

        modalVm.OrderItems.Clear();
        modalVm.OrderItems.Add(new SalesOrderLineItemViewModel
        {
            ProductName = "Test Item 1",
            Quantity = 2m,
            UnitPrice = 100m,
            UnitCost = 60m
        }); // Total: 200

        modalVm.OrderItems.Add(new SalesOrderLineItemViewModel
        {
            ProductName = "Test Item 2",
            Quantity = 1m,
            UnitPrice = 1000m,
            UnitCost = 800m
        }); // Total: 1000

        modalVm.TaxRatePercent = 10m; // 10% Tax

        Assert.Equal(1200m, modalVm.SubTotal);
        Assert.Equal(120m, modalVm.TaxAmount);
        Assert.Equal(1320m, modalVm.GrandTotal);
    }

    [Fact]
    public void SalesOrderLineItem_FlagsLowGrossMargin()
    {
        var itemHighMargin = new SalesOrderLineItemViewModel
        {
            UnitPrice = 100m,
            UnitCost = 60m // Margin 40%
        };

        var itemLowMargin = new SalesOrderLineItemViewModel
        {
            UnitPrice = 100m,
            UnitCost = 90m // Margin 10% < 20%
        };

        Assert.False(itemHighMargin.IsLowMargin);
        Assert.True(itemLowMargin.IsLowMargin);
    }

    [Fact]
    public async Task SalesOrderBuilder_SubmitOrder_DispatchesMediatRCommand()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<CreateSalesOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        bool notificationDispatched = false;
        bool closeRequested = false;

        var modalVm = new SalesOrderBuilderModalViewModel(
            mockMediator.Object,
            isQuotationConversion: false,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        await modalVm.SubmitOrderAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<CreateSalesOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }

    [Fact]
    public async Task SalesViewModel_MoveDealStage_UpdatesKanbanStage()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<YAERP.Application.Sales.Commands.UpdateDealStage.UpdateDealStageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var salesVm = new SalesViewModel(mockMediator.Object);
        await salesVm.RefreshSalesDataAsync();

        var firstDeal = salesVm.KanbanDeals[0];
        Assert.Equal("Qualification", firstDeal.Stage);

        await salesVm.MoveDealStageAsync(firstDeal);

        Assert.Equal("Proposal", salesVm.KanbanDeals[0].Stage);
    }
}
