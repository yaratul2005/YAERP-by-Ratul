using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;
using YAERP.Application.Sales.Commands.CreateSalesOrder;

namespace YAERP.Application.Sales.Commands.ConvertQuoteToSalesOrder;

public record ConvertQuoteToSalesOrderCommand(
    Guid QuoteId,
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    List<SalesOrderItemDto> Items) : ICommand<Guid>;
