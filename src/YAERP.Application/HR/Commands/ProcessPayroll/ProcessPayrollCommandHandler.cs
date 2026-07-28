using System;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;

namespace YAERP.Application.HR.Commands.ProcessPayroll;

public class ProcessPayrollCommandHandler : ICommandHandler<ProcessPayrollCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public ProcessPayrollCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(ProcessPayrollCommand request, CancellationToken cancellationToken)
    {
        // Mocked out because we deleted Domain.HR.Payroll
        // In Step E2, we'll create the correct PayrollRun integration
        return Result<Guid>.Success(Guid.NewGuid());
    }
}
