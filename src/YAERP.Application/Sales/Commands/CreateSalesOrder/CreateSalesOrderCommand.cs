using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Sales.Commands.CreateSalesOrder;

public record SalesOrderItemDto(Guid ProductId, decimal Quantity, decimal UnitPrice);

public record CreateSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    List<SalesOrderItemDto> Items) : ICommand<Guid>;
