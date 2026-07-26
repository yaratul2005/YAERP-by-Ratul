using System.Threading.Tasks;
using YAERP.Application.Finance.DTOs;
using YAERP.Application.Purchasing.DTOs;

namespace YAERP.Application.Common.Interfaces.Reporting;

public interface IPdfReportGenerator
{
    Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice);
    Task<byte[]> GeneratePurchaseOrderPdfAsync(PurchaseOrderDto purchaseOrder);
}
