using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;
using YAERP.Domain.Purchasing;

namespace YAERP.Application.Purchasing.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler : ICommandHandler<CreatePurchaseOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreatePurchaseOrderCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.PurchaseOrders.AnyAsync(po => po.OrderNumber == request.OrderNumber, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("PurchaseOrder.Duplicate", "Order number must be unique.", ErrorType.Conflict));

        var po = PurchaseOrder.Create(
            tenantId,
            new VendorId(request.VendorId),
            new WarehouseId(request.WarehouseId),
            request.OrderNumber,
            request.ExpectedDeliveryDateUtc);

        foreach (var item in request.Items)
        {
            po.AddItem(new ProductId(item.ProductId), item.Quantity, item.UnitPrice);
        }

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(po.Id.Value);
    }
}
