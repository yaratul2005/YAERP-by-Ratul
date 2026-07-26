using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.HR;

namespace YAERP.Application.HR.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateEmployeeCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var exists = await _context.Employees.AnyAsync(e => e.EmployeeCode == request.EmployeeCode, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(new Error("Employee.Duplicate", "Employee code must be unique.", ErrorType.Conflict));

        var employee = Employee.Create(
            tenantId,
            request.EmployeeCode,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Department,
            request.BaseSalary);

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(employee.Id.Value);
    }
}
