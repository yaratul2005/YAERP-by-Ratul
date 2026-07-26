using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Sales.Commands.FulfillSalesOrder;

public record FulfillSalesOrderCommand(Guid SalesOrderId) : ICommand;
