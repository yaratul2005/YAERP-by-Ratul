using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Purchasing.Commands.ReceivePurchaseOrder;

public record ReceivePurchaseOrderCommand(Guid PurchaseOrderId) : ICommand;
