using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Financials.Commands.PostDepreciationRun;

public record PostDepreciationRunCommand(
    Guid AssetId,
    string Period,
    decimal Amount,
    string DepreciationMethod) : ICommand<bool>;
