using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using YAERP.Application.Common.Interfaces;
using YAERP.UI.Services;
using YAERP.UI.ViewModels.Modals;

namespace YAERP.UI.Tests;

public class GoogleSheetsExportModalViewModelTests
{
    [Fact]
    public async Task ExportAndOpenAsync_ShouldTriggerExportServiceAndCloseModal()
    {
        // Arrange
        var authMock = new Mock<IGoogleAuthService>();
        var driveMock = new Mock<IGoogleDriveService>();
        var sheetsMock = new Mock<IGoogleSheetsService>();
        var modalMock = new Mock<IModalService>();
        var dialogMock = new Mock<IDialogService>();

        authMock.Setup(a => a.IsConnected).Returns(true);
        sheetsMock.Setup(s => s.ExportDataGridToSheetAsync(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<List<object>>>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            default))
            .ReturnsAsync("https://docs.google.com/spreadsheets/d/mock-id/edit");

        var headers = new List<string> { "Col1", "Col2" };
        var rows = new List<List<object>> { new() { "Val1", "Val2" } };

        var vm = new GoogleSheetsExportModalViewModel(
            authMock.Object,
            driveMock.Object,
            sheetsMock.Object,
            modalMock.Object,
            dialogMock.Object,
            "Test Spreadsheet",
            headers,
            rows);

        // Act
        await vm.ExportAndOpenCommand.ExecuteAsync(null);

        // Assert
        sheetsMock.Verify(s => s.ExportDataGridToSheetAsync(
            "Test Spreadsheet",
            headers,
            rows,
            It.IsAny<string>(),
            true,
            true,
            true,
            default), Times.Once);

        modalMock.Verify(m => m.CloseModal(), Times.Once);
    }
}
