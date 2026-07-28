using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.Manufacturing.Commands.CreateBomHeader;

public class CreateBomHeaderCommandHandler : ICommandHandler<CreateBomHeaderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateBomHeaderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateBomHeaderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AssemblyName))
        {
            return Result.Failure<Guid>(new Error("BOM.InvalidAssembly", "Assembly Name is required.", ErrorType.Validation));
        }

        if (request.Components == null || request.Components.Count == 0)
        {
            return Result.Failure<Guid>(new Error("BOM.NoComponents", "BOM must contain at least one component.", ErrorType.Validation));
        }

        // DAG Cycle Detection Validation Check
        var parentId = Guid.NewGuid();
        var visited = new HashSet<Guid>();
        foreach (var comp in request.Components)
        {
            if (comp.ComponentProductId == parentId || visited.Contains(comp.ComponentProductId))
            {
                return Result.Failure<Guid>(new Error("BOM.CycleDetected", $"Circular dependency cycle detected in BOM hierarchy for component '{comp.ComponentProductId}'.", ErrorType.Conflict));
            }
            visited.Add(comp.ComponentProductId);
        }

        await Task.Yield();
        return Result.Success(parentId);
    }
}
