using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Sales.DTOs;

namespace YAERP.Application.Common.Interfaces;

public interface IReceiptPrinterService
{
    Task<bool> PrintSalesReceiptAsync(SalesReceiptDto receipt, string printerName, CancellationToken cancellationToken = default);
    Task<bool> OpenCashDrawerAsync(string printerName, CancellationToken cancellationToken = default);
}
