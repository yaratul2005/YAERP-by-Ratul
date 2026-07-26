using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Sales.DTOs;

namespace YAERP.Infrastructure.Hardware.Services;

/// <summary>
/// Hardware interop service for ESC/POS thermal receipt printing and cash drawer kick pulse.
/// Uses Win32 winspool RAW printing stream with graceful fallback & exception handling.
/// </summary>
public class ReceiptPrinterService : IReceiptPrinterService
{
    private readonly ILogger<ReceiptPrinterService> _logger;

    public ReceiptPrinterService(ILogger<ReceiptPrinterService> logger)
    {
        _logger = logger;
    }

    public Task<bool> PrintSalesReceiptAsync(
        SalesReceiptDto receipt,
        string printerName,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Encoding ESC/POS receipt #{ReceiptNumber} for printer '{PrinterName}'",
                    receipt.ReceiptNumber, printerName);

                byte[] bytes = EscPosEncoder.EncodeReceipt(receipt);

                if (string.Equals(printerName, "MOCK", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(printerName))
                {
                    _logger.LogInformation("Mock printer target specified. Successfully generated {Length} ESC/POS bytes.", bytes.Length);
                    return true;
                }

                bool success = RawPrinterHelper.SendBytesToPrinter(printerName, bytes, $"YAERP_Receipt_{receipt.ReceiptNumber}");
                if (!success)
                {
                    _logger.LogWarning("Failed to send raw ESC/POS bytes to Windows printer '{PrinterName}'. Printer may be offline.", printerName);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hardware error while printing receipt #{ReceiptNumber} on '{PrinterName}'", receipt.ReceiptNumber, printerName);
                return false;
            }
        }, cancellationToken);
    }

    public Task<bool> OpenCashDrawerAsync(
        string printerName,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Sending ESC/POS cash drawer pulse to printer '{PrinterName}'", printerName);

                byte[] bytes = EscPosEncoder.EncodeOpenCashDrawer();

                if (string.Equals(printerName, "MOCK", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(printerName))
                {
                    _logger.LogInformation("Mock printer target specified. Cash drawer pulse generated successfully.");
                    return true;
                }

                bool success = RawPrinterHelper.SendBytesToPrinter(printerName, bytes, "YAERP_OpenCashDrawer");
                if (!success)
                {
                    _logger.LogWarning("Failed to send cash drawer pulse to Windows printer '{PrinterName}'.", printerName);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hardware error sending cash drawer pulse to '{PrinterName}'", printerName);
                return false;
            }
        }, cancellationToken);
    }
}

internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes, string docName)
    {
        if (string.IsNullOrWhiteSpace(szPrinterName)) return false;

        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
        Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);

        try
        {
            if (!OpenPrinter(szPrinterName, out var hPrinter, IntPtr.Zero))
            {
                return false;
            }

            var di = new DOCINFOA
            {
                pDocName = docName,
                pDataType = "RAW"
            };

            bool success = false;
            if (StartDocPrinter(hPrinter, 1, di))
            {
                if (StartPagePrinter(hPrinter))
                {
                    success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out _);
                    EndPagePrinter(hPrinter);
                }
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
            return success;
        }
        catch
        {
            return false;
        }
        finally
        {
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
        }
    }
}
