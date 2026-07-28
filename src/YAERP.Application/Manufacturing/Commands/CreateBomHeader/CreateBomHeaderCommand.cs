using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Manufacturing.Commands.CreateBomHeader;

public record BomComponentDto(Guid ComponentProductId, decimal QuantityPerAssembly, decimal YieldPercent, decimal ScrapFactorPercent);

public record CreateBomHeaderCommand(
    string AssemblyName,
    string RevisionCode,
    List<BomComponentDto> Components) : ICommand<Guid>;
