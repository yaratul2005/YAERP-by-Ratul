using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Manufacturing.Commands.RecordProductionOutput;

public record RecordProductionOutputCommand(
    Guid WorkOrderId,
    decimal CompletedQuantity,
    decimal ScrappedQuantity,
    string ScrapReason,
    decimal LaborHours) : ICommand<bool>;
