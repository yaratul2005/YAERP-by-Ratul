using System.IO;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Finance.DTOs;
using YAERP.Application.Purchasing.DTOs;

namespace YAERP.Infrastructure.Reporting;

public class QuestPdfReportGenerator : IPdfReportGenerator
{
    public QuestPdfReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Invoice #{invoice.InvoiceNumber}").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content().Column(x =>
                {
                    x.Spacing(20);
                    x.Item().Text($"Due Date: {invoice.DueDateUtc:d}");
                    x.Item().Text($"Total Amount: {invoice.TotalAmount:C}");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GeneratePurchaseOrderPdfAsync(PurchaseOrderDto purchaseOrder)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Purchase Order #{purchaseOrder.OrderNumber}").SemiBold().FontSize(20).FontColor(Colors.Green.Darken2);

                page.Content().Column(x =>
                {
                    x.Spacing(20);
                    x.Item().Text($"Order Date: {purchaseOrder.OrderDateUtc:d}");
                    x.Item().Text($"Total Amount: {purchaseOrder.TotalAmount:C}");
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
