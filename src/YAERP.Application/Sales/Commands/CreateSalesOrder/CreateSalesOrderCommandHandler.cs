using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;
using YAERP.Domain.Sales;

namespace YAERP.Application.Sales.Commands.CreateSalesOrder;

public class CreateSalesOrderCommandHandler : ICommandHandler<CreateSalesOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateSalesOrderCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.SalesOrders.AnyAsync(so => so.OrderNumber == request.OrderNumber, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("SalesOrder.Duplicate", "Order number must be unique.", ErrorType.Conflict));

        var so = SalesOrder.Create(
            tenantId,
            new CustomerId(request.CustomerId),
            new WarehouseId(request.WarehouseId),
            request.OrderNumber);

        foreach (var item in request.Items)
        {
            so.AddItem(new ProductId(item.ProductId), item.Quantity, item.UnitPrice);
        }

        _context.SalesOrders.Add(so);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(so.Id.Value);
    }
}
