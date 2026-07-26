using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using YAERP.Application.Sales.DTOs;
using YAERP.Infrastructure.Hardware;

namespace YAERP.Infrastructure.Tests.Hardware;

public class EscPosEncoderTests
{
    [Fact]
    public void EncodeOpenCashDrawer_Should_GenerateDrawerPulseCommand()
    {
        // Act
        byte[] bytes = EscPosEncoder.EncodeOpenCashDrawer();

        // Assert
        Assert.NotNull(bytes);
        Assert.Equal(7, bytes.Length);

        // Verify Init bytes (0x1B, 0x40)
        Assert.Equal(0x1B, bytes[0]);
        Assert.Equal(0x40, bytes[1]);

        // Verify Open Cash Drawer Pulse bytes (0x1B, 0x70, 0x00, 0x19, 0xFA)
        Assert.Equal(0x1B, bytes[2]);
        Assert.Equal(0x70, bytes[3]);
        Assert.Equal(0x00, bytes[4]);
        Assert.Equal(0x19, bytes[5]);
        Assert.Equal(0xFA, bytes[6]);
    }

    [Fact]
    public void EncodeReceipt_Should_GenerateValidBinaryStreamWithESCPOSTokens()
    {
        // Arrange
        var items = new List<SalesReceiptItemDto>
        {
            new("Wireless Mouse", 2, 25.00m, 50.00m),
            new("Mechanical Keyboard", 1, 120.00m, 120.00m)
        };

        var receipt = new SalesReceiptDto(
            ReceiptNumber: "REC-9001",
            TransactionDate: new DateTime(2026, 7, 26, 14, 30, 0, DateTimeKind.Utc),
            CashierName: "Ratul System",
            CustomerName: "Acme Corp",
            LineItems: items,
            SubTotal: 170.00m,
            TaxAmount: 17.00m,
            TotalAmount: 187.00m,
            PaymentMethod: "Credit Card");

        // Act
        byte[] bytes = EscPosEncoder.EncodeReceipt(receipt, columnWidth: 42);

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 100);

        string asciiText = Encoding.ASCII.GetString(bytes);

        Assert.Contains("YAERP ENTERPRISE STORE", asciiText);
        Assert.Contains("REC-9001", asciiText);
        Assert.Contains("Acme Corp", asciiText);
        Assert.Contains("Wireless Mouse", asciiText);
        Assert.Contains("Mechanical Keyb", asciiText);
        Assert.Contains("Credit Card", asciiText);

        // Verify key ESC/POS binary tokens exist in the output stream
        Assert.Contains(EscPosEncoder.CmdInit[0], bytes);
        Assert.Contains(EscPosEncoder.CmdInit[1], bytes);
        Assert.Contains(EscPosEncoder.CmdCutPaper[0], bytes);
        Assert.Contains(EscPosEncoder.CmdCutPaper[1], bytes);
        Assert.Contains(EscPosEncoder.CmdOpenCashDrawer[0], bytes);
        Assert.Contains(EscPosEncoder.CmdOpenCashDrawer[1], bytes);
    }
}
