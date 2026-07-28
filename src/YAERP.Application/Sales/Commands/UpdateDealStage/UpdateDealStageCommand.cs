using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Sales.Commands.UpdateDealStage;

public record UpdateDealStageCommand(Guid DealId, string NewStage) : ICommand<bool>;
