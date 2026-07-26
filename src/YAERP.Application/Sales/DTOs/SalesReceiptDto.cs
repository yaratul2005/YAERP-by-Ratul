using System;
using System.Collections.Generic;

namespace YAERP.Application.Sales.DTOs;

public record SalesReceiptItemDto(
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record SalesReceiptDto(
    string ReceiptNumber,
    DateTime TransactionDate,
    string CashierName,
    string CustomerName,
    IReadOnlyList<SalesReceiptItemDto> LineItems,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string PaymentMethod);
