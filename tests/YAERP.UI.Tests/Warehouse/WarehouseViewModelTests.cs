using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Warehouse.Commands.ReceiveInventoryShipment;
using YAERP.Application.Warehouse.Commands.TransferStockBetweenBins;
using YAERP.Domain.Common.Primitives;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Drawers;
using YAERP.UI.ViewModels.Modals;

namespace YAERP.UI.Tests.Warehouse;

public class WarehouseViewModelTests
{
    [Fact]
    public async Task BinTransferDrawer_ExecuteTransfer_DispatchesMediatRCommand()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<TransferStockBetweenBinsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var bin1 = new WarehouseBinDto(Guid.NewGuid(), "BIN-01", "Picking", 50m, false, false);
        var bin2 = new WarehouseBinDto(Guid.NewGuid(), "BIN-02", "Bulk", 20m, false, false);
        var bins = new ObservableCollection<WarehouseBinDto> { bin1, bin2 };

        bool notificationDispatched = false;
        bool closeRequested = false;

        var drawerVm = new BinTransferDrawerViewModel(
            mockMediator.Object,
            bins,
            bin1,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        drawerVm.SelectedSourceBin = bin1;
        drawerVm.SelectedDestinationBin = bin2;
        drawerVm.Quantity = 50m;

        await drawerVm.ExecuteTransferAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<TransferStockBetweenBinsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }

    [Fact]
    public async Task GoodsReceivingModal_CalculateSlotting_PopulatesRecommendedBins()
    {
        var mockMediator = new Mock<IMediator>();
        var mockSlotting = new Mock<ISlottingOptimizationService>();

        mockSlotting.Setup(s => s.RecommendBinsAsync(
            It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new System.Collections.Generic.List<SlottingRecommendation>
            {
                new SlottingRecommendation(Guid.NewGuid(), "BIN-REC-A1", 5.0m, 300m)
            });

        var modalVm = new GoodsReceivingModalViewModel(mockMediator.Object, mockSlotting.Object);

        await modalVm.CalculateSlottingAsync();

        Assert.Single(modalVm.RecommendedBins);
        Assert.Equal("BIN-REC-A1", modalVm.RecommendedBins[0].BinCode);
    }

    [Fact]
    public async Task GoodsReceivingModal_SubmitReceiving_DispatchesMediatRCommand()
    {
        var mockMediator = new Mock<IMediator>();
        var mockSlotting = new Mock<ISlottingOptimizationService>();

        mockMediator.Setup(m => m.Send(It.IsAny<ReceiveInventoryShipmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        bool notificationDispatched = false;
        bool closeRequested = false;

        var modalVm = new GoodsReceivingModalViewModel(
            mockMediator.Object,
            mockSlotting.Object,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        await modalVm.SubmitReceivingAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<ReceiveInventoryShipmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }
}
