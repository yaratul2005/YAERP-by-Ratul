using System;
using System.IO;
using System.Text;
using YAERP.Application.Sales.DTOs;

namespace YAERP.Infrastructure.Hardware;

/// <summary>
/// ESC/POS binary command encoder for thermal receipt printers (58mm / 80mm) and cash drawers.
/// Generates raw byte streams complying with standard Epson ESC/POS specification.
/// </summary>
public static class EscPosEncoder
{
    // ESC/POS Command Constants
    public static readonly byte[] CmdInit = { 0x1B, 0x40 };
    public static readonly byte[] CmdAlignLeft = { 0x1B, 0x61, 0x00 };
    public static readonly byte[] CmdAlignCenter = { 0x1B, 0x61, 0x01 };
    public static readonly byte[] CmdAlignRight = { 0x1B, 0x61, 0x02 };
    public static readonly byte[] CmdBoldOn = { 0x1B, 0x45, 0x01 };
    public static readonly byte[] CmdBoldOff = { 0x1B, 0x45, 0x00 };
    public static readonly byte[] CmdCutPaper = { 0x1D, 0x56, 0x42, 0x00 };
    public static readonly byte[] CmdOpenCashDrawer = { 0x1B, 0x70, 0x00, 0x19, 0xFA };

    public static byte[] EncodeOpenCashDrawer()
    {
        using var ms = new MemoryStream();
        ms.Write(CmdInit, 0, CmdInit.Length);
        ms.Write(CmdOpenCashDrawer, 0, CmdOpenCashDrawer.Length);
        return ms.ToArray();
    }

    public static byte[] EncodeReceipt(SalesReceiptDto receipt, int columnWidth = 42)
    {
        using var ms = new MemoryStream();

        // 1. Initialize Printer
        WriteBytes(ms, CmdInit);

        // 2. Header
        WriteBytes(ms, CmdAlignCenter);
        WriteBytes(ms, CmdBoldOn);
        WriteLine(ms, "YAERP ENTERPRISE STORE");
        WriteBytes(ms, CmdBoldOff);
        WriteLine(ms, "Retail & Point of Sale Station");
        WriteLine(ms, new string('-', columnWidth));

        // 3. Metadata
        WriteBytes(ms, CmdAlignLeft);
        WriteLine(ms, $"Receipt #:  {receipt.ReceiptNumber}");
        WriteLine(ms, $"Date:       {receipt.TransactionDate:yyyy-MM-dd HH:mm:ss}");
        WriteLine(ms, $"Cashier:    {receipt.CashierName}");
        WriteLine(ms, $"Customer:   {receipt.CustomerName}");
        WriteLine(ms, new string('-', columnWidth));

        // 4. Line Items Table Header
        WriteBytes(ms, CmdBoldOn);
        WriteLine(ms, FormatLineItem("Item Description", "Qty", "Price", "Total", columnWidth));
        WriteBytes(ms, CmdBoldOff);
        WriteLine(ms, new string('-', columnWidth));

        // 5. Line Items
        foreach (var item in receipt.LineItems)
        {
            string itemName = item.ItemName.Length > 18 ? item.ItemName.Substring(0, 18) : item.ItemName;
            string qtyStr = item.Quantity.ToString("N0");
            string priceStr = item.UnitPrice.ToString("F2");
            string totalStr = item.LineTotal.ToString("F2");

            WriteLine(ms, FormatLineItem(itemName, qtyStr, priceStr, totalStr, columnWidth));
        }

        WriteLine(ms, new string('-', columnWidth));

        // 6. Totals & Payment Method
        WriteBytes(ms, CmdAlignRight);
        WriteLine(ms, $"Subtotal:   {receipt.SubTotal:C2}");
        WriteLine(ms, $"Tax (VAT):  {receipt.TaxAmount:C2}");

        WriteBytes(ms, CmdBoldOn);
        WriteLine(ms, $"TOTAL:      {receipt.TotalAmount:C2}");
        WriteBytes(ms, CmdBoldOff);

        WriteLine(ms, $"Paid via:   {receipt.PaymentMethod}");
        WriteLine(ms, new string('-', columnWidth));

        // 7. Footer
        WriteBytes(ms, CmdAlignCenter);
        WriteLine(ms, "Thank you for your business!");
        WriteLine(ms, "Powered by YAERP Suite");
        WriteLine(ms, "\n\n");

        // 8. Open Cash Drawer Pulse & Cut Paper
        WriteBytes(ms, CmdOpenCashDrawer);
        WriteBytes(ms, CmdCutPaper);

        return ms.ToArray();
    }

    private static string FormatLineItem(string name, string qty, string price, string total, int width)
    {
        // Allocation: Name (18), Qty (5), Price (9), Total (10) = 42 cols
        string nameCol = name.PadRight(18);
        string qtyCol = qty.PadLeft(5);
        string priceCol = price.PadLeft(9);
        string totalCol = total.PadLeft(10);

        string line = $"{nameCol}{qtyCol}{priceCol}{totalCol}";
        if (line.Length > width)
        {
            return line.Substring(0, width);
        }
        return line;
    }

    private static void WriteLine(MemoryStream ms, string text)
    {
        byte[] data = Encoding.ASCII.GetBytes(text + "\n");
        ms.Write(data, 0, data.Length);
    }

    private static void WriteBytes(MemoryStream ms, byte[] bytes)
    {
        ms.Write(bytes, 0, bytes.Length);
    }
}
