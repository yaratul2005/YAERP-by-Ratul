using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Sales;

namespace YAERP.Application.Sales.Commands.UpdateCustomerProfile;

public class UpdateCustomerProfileCommandHandler : ICommandHandler<UpdateCustomerProfileCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        var customerId = new CustomerId(request.CustomerId);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer == null)
        {
            return Result.Failure<bool>(new Error("Customer.NotFound", "Customer not found.", ErrorType.NotFound));
        }

        if (request.CreditLimit < 0)
        {
            return Result.Failure<bool>(new Error("Customer.InvalidCreditLimit", "Credit limit cannot be negative.", ErrorType.Validation));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
