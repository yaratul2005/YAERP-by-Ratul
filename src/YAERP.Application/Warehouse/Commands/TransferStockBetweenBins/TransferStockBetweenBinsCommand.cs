using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Warehouse.Commands.TransferStockBetweenBins;

public record TransferStockBetweenBinsCommand(
    Guid SourceBinId,
    Guid DestinationBinId,
    Guid ProductId,
    decimal Quantity,
    string Notes) : ICommand<bool>;
