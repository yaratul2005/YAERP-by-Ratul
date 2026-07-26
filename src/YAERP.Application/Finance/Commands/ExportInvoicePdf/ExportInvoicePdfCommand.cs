using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Finance.Commands.ExportInvoicePdf;

public record ExportInvoicePdfCommand(Guid InvoiceId) : ICommand<byte[]>;
