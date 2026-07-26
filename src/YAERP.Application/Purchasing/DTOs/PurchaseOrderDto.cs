using System;

namespace YAERP.Application.Purchasing.DTOs;

public record PurchaseOrderDto(Guid PurchaseOrderId, string OrderNumber, decimal TotalAmount, DateTime OrderDateUtc);
