using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;
using YAERP.Application.Finance.DTOs;

namespace YAERP.Application.Finance.Commands.ExportInvoicePdf;

public class ExportInvoicePdfCommandHandler : ICommandHandler<ExportInvoicePdfCommand, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfReportGenerator _pdfGenerator;

    public ExportInvoicePdfCommandHandler(IApplicationDbContext context, IPdfReportGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<Result<byte[]>> Handle(ExportInvoicePdfCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == new InvoiceId(request.InvoiceId), cancellationToken);

        if (invoice == null)
            return Result.Failure<byte[]>(new Error("Invoice.NotFound", "Invoice not found.", ErrorType.NotFound));

        var dto = new InvoiceDto(invoice.Id.Value, invoice.InvoiceNumber, invoice.TotalAmount, invoice.DueDateUtc);

        var pdfBytes = await _pdfGenerator.GenerateInvoicePdfAsync(dto);

        return Result.Success(pdfBytes);
    }
}
