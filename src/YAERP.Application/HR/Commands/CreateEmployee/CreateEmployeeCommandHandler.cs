using System;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Entities.Hcm;

namespace YAERP.Application.HR.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateEmployeeCommandHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId
            ?? throw new UnauthorizedAccessException("Tenant context required.");

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = request.EmployeeCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Department = request.Department,
            JobTitle = request.JobTitle,
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            BaseSalary = request.BaseSalary,
            PayFrequency = request.PayFrequency
        };

        _dbContext.Set<Employee>().Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(employee.Id);
    }
}
