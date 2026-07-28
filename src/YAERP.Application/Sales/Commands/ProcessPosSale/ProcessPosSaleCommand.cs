using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Sales.Commands.ProcessPosSale;

public record PosCartLineDto(Guid ProductId, string SKU, string Name, decimal Quantity, decimal UnitPrice);

public record ProcessPosSaleCommand(
    string ReceiptNumber,
    decimal SubTotal,
    decimal TaxAmount,
    decimal GrandTotal,
    string PaymentMethod,
    decimal CashTendered,
    decimal ChangeDue,
    List<PosCartLineDto> LineItems) : ICommand<Guid>;
