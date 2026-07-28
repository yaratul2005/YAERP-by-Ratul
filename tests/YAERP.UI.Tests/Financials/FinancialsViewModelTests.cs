using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Financials.Commands.PostJournalEntry;
using YAERP.Application.Sales.Commands.ProcessPosSale;
using YAERP.Domain.Common.Primitives;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Modals;

namespace YAERP.UI.Tests.Financials;

public class FinancialsViewModelTests
{
    [Fact]
    public void JournalEntryModal_BlocksSubmission_WhenUnbalanced()
    {
        var mockMediator = new Mock<IMediator>();
        var modalVm = new JournalEntryModalViewModel(mockMediator.Object);

        modalVm.JournalLines.Clear();
        modalVm.JournalLines.Add(new JournalEntryLineViewModel { Debit = 1000m, Credit = 0m });
        modalVm.JournalLines.Add(new JournalEntryLineViewModel { Debit = 0m, Credit = 800m }); // Unbalanced

        Assert.True(modalVm.IsUnbalanced);
        Assert.False(modalVm.IsBalanced);
        Assert.Equal(200m, modalVm.UnbalancedDifference);
    }

    [Fact]
    public async Task JournalEntryModal_Submit_DispatchesMediatRCommand_WhenBalanced()
    {
        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<PostJournalEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        bool notificationDispatched = false;
        bool closeRequested = false;

        var modalVm = new JournalEntryModalViewModel(
            mockMediator.Object,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        // Balanced by default sample lines ($5,000 Debit == $5,000 Credit)
        Assert.True(modalVm.IsBalanced);

        await modalVm.PostJournalAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<PostJournalEntryCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }

    [Fact]
    public void CashTenderModal_CalculatesChangeDueCorrectly()
    {
        var mockMediator = new Mock<IMediator>();
        var mockPrinter = new Mock<IReceiptPrinterService>();
        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(Guid.NewGuid(), "SKU-01", "Item 1", 1, 50m)
        };

        var modalVm = new CashTenderModalViewModel(mockMediator.Object, mockPrinter.Object, 55m, 50m, 5m, cartItems);

        modalVm.CashTendered = 100m;
        Assert.Equal(45m, modalVm.ChangeDue);
        Assert.True(modalVm.IsSufficient);

        modalVm.CashTendered = 40m;
        Assert.Equal(0m, modalVm.ChangeDue);
        Assert.False(modalVm.IsSufficient);
    }

    [Fact]
    public async Task CashTenderModal_CompleteSale_DispatchesMediatRAndHardwareServices()
    {
        var mockMediator = new Mock<IMediator>();
        var mockPrinter = new Mock<IReceiptPrinterService>();

        mockMediator.Setup(m => m.Send(It.IsAny<ProcessPosSaleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        bool notificationDispatched = false;
        bool closeRequested = false;

        var cartItems = new List<CartItemDto>
        {
            new CartItemDto(Guid.NewGuid(), "SKU-01", "Item 1", 2, 25m)
        };

        var modalVm = new CashTenderModalViewModel(
            mockMediator.Object,
            mockPrinter.Object,
            55m,
            50m,
            5m,
            cartItems,
            onSuccessNotification: msg => notificationDispatched = true,
            onCloseRequested: () => closeRequested = true);

        modalVm.CashTendered = 100m;

        await modalVm.CompleteSaleAsync();

        mockMediator.Verify(m => m.Send(It.IsAny<ProcessPosSaleCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        mockPrinter.Verify(p => p.PrintSalesReceiptAsync(It.IsAny<YAERP.Application.Sales.DTOs.SalesReceiptDto>(), It.IsAny<string>()), Times.Once);
        mockPrinter.Verify(p => p.OpenCashDrawerAsync(It.IsAny<string>()), Times.Once);

        Assert.True(notificationDispatched);
        Assert.True(closeRequested);
    }
}
