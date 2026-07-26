using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Inventory;

namespace YAERP.Application.Inventory.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateProductCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.Products.AnyAsync(p => p.SKU == request.SKU, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("Product.DuplicateSKU", "SKU must be unique.", ErrorType.Conflict));

        var product = Product.Create(
            tenantId,
            request.SKU,
            request.Barcode,
            request.Name,
            request.Description,
            request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : null,
            new UnitOfMeasureId(request.UnitOfMeasureId),
            request.StandardCost,
            request.ListPrice,
            request.IsBatchTracked);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id.Value);
    }
}
