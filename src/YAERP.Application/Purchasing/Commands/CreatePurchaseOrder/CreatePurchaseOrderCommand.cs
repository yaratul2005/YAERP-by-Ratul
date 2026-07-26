using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Purchasing.Commands.CreatePurchaseOrder;

public record PurchaseOrderItemDto(Guid ProductId, decimal Quantity, decimal UnitPrice);

public record CreatePurchaseOrderCommand(
    Guid VendorId,
    Guid WarehouseId,
    string OrderNumber,
    DateTime? ExpectedDeliveryDateUtc,
    List<PurchaseOrderItemDto> Items) : ICommand<Guid>;
