using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Finance.Commands.CreateInvoiceFromSalesOrder;

public record CreateInvoiceFromSalesOrderCommand(
    Guid SalesOrderId,
    string InvoiceNumber,
    DateTime DueDateUtc) : ICommand<Guid>;
