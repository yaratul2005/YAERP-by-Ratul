using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YAERP.Application.Sales.DTOs;

namespace YAERP.Infrastructure.Hardware.Services;

public class EscPosPrinterDriver
{
    private static readonly byte[] CmdInit = new byte[] { 0x1B, 0x40 }; // ESC @
    private static readonly byte[] CmdCutFull = new byte[] { 0x1D, 0x56, 0x41, 0x00 }; // GS V 65 0
    private static readonly byte[] CmdCutPartial = new byte[] { 0x1D, 0x56, 0x01 }; // GS V 1
    private static readonly byte[] CmdDrawerKickPin2 = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA }; // ESC p 0 25 250
    private static readonly byte[] CmdDrawerKickPin5 = new byte[] { 0x1B, 0x70, 0x01, 0x19, 0xFA }; // ESC p 1 25 250

    public static byte[] BuildReceiptBytes(SalesReceiptDto receipt)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.ASCII);

        // Initialize printer
        bw.Write(CmdInit);

        // Center Alignment & Double Height Header
        bw.Write(new byte[] { 0x1B, 0x61, 0x01 }); // Center align
        bw.Write(new byte[] { 0x1B, 0x21, 0x20 }); // Double height
        bw.Write(Encoding.ASCII.GetBytes("YAERP ENTERPRISE POS\n"));
        bw.Write(new byte[] { 0x1B, 0x21, 0x00 }); // Normal font
        bw.Write(Encoding.ASCII.GetBytes("Official Sales Receipt\n"));
        bw.Write(Encoding.ASCII.GetBytes($"Receipt #: {receipt.ReceiptNumber}\n"));
        bw.Write(Encoding.ASCII.GetBytes($"Date: {receipt.TransactionDate:yyyy-MM-dd HH:mm:ss}\n"));
        bw.Write(Encoding.ASCII.GetBytes("------------------------------------------\n"));

        // Left Alignment for Line Items
        bw.Write(new byte[] { 0x1B, 0x61, 0x00 }); // Left align
        foreach (var item in receipt.LineItems)
        {
            string line = $"{item.ItemName.PadRight(22).Substring(0, 22)} x{item.Quantity,-3} ${item.LineTotal,7:F2}\n";
            bw.Write(Encoding.ASCII.GetBytes(line));
        }

        bw.Write(Encoding.ASCII.GetBytes("------------------------------------------\n"));

        // Right Alignment for Totals
        bw.Write(new byte[] { 0x1B, 0x61, 0x02 }); // Right align
        bw.Write(Encoding.ASCII.GetBytes($"Subtotal: ${receipt.SubTotal:F2}\n"));
        bw.Write(Encoding.ASCII.GetBytes($"Sales Tax (10%): ${receipt.TaxAmount:F2}\n"));

        bw.Write(new byte[] { 0x1B, 0x21, 0x08 }); // Bold font
        bw.Write(Encoding.ASCII.GetBytes($"TOTAL: ${receipt.TotalAmount:F2}\n"));
        bw.Write(new byte[] { 0x1B, 0x21, 0x00 }); // Normal font

        bw.Write(Encoding.ASCII.GetBytes($"Payment Method: {receipt.PaymentMethod}\n\n"));

        // Center Align Footer
        bw.Write(new byte[] { 0x1B, 0x61, 0x01 });
        bw.Write(Encoding.ASCII.GetBytes("Thank you for shopping with YAERP!\n\n\n\n"));

        // Paper Feed & Cut
        bw.Write(CmdCutFull);

        return ms.ToArray();
    }

    public static async Task SendToNetworkPrinterAsync(string ipAddress, int port, byte[] data)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(ipAddress, port);
        using var stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }

    public static byte[] GetDrawerKickBytes()
    {
        using var ms = new MemoryStream();
        ms.Write(CmdInit, 0, CmdInit.Length);
        ms.Write(CmdDrawerKickPin2, 0, CmdDrawerKickPin2.Length);
        return ms.ToArray();
    }
}
