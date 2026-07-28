using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using DomainWarehouse = YAERP.Domain.Inventory.Warehouse;

namespace YAERP.Application.Inventory.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateWarehouseCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.Warehouses.AnyAsync(w => w.Code == request.Code, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("Warehouse.DuplicateCode", "Warehouse code must be unique.", ErrorType.Conflict));

        var warehouse = DomainWarehouse.Create(
            tenantId,
            request.Code,
            request.Name,
            request.Address);

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(warehouse.Id.Value);
    }
}
