using System.Collections.Generic;

namespace YAERP.Application.Sales.DTOs;

public record ExternalOrderItemDto(
    string SKU,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record ExternalOrderWebhookDto(
    string ExternalOrderId,
    string PlatformSource,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    decimal TaxAmount,
    string PaymentStatus,
    string FulfillmentStatus,
    IReadOnlyList<ExternalOrderItemDto> LineItems);
