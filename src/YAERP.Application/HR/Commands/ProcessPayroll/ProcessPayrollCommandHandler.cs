using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Finance;
using YAERP.Domain.HR;

namespace YAERP.Application.HR.Commands.ProcessPayroll;

public class ProcessPayrollCommandHandler : ICommandHandler<ProcessPayrollCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ProcessPayrollCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(ProcessPayrollCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.CurrentTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(new Error("Tenant.Missing", "No tenant context found.", ErrorType.Failure));

        var empId = new EmployeeId(request.EmployeeId);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId, cancellationToken);

        if (employee == null)
            return Result.Failure<Guid>(new Error("Employee.NotFound", "Employee not found.", ErrorType.NotFound));

        var payroll = Payroll.Create(
            tenantId,
            empId,
            request.PeriodStart,
            request.PeriodEnd,
            request.GrossSalary,
            request.TaxDeductions);

        payroll.MarkAsProcessed();
        _context.Payrolls.Add(payroll);

        // Generate Journal Entry
        var je = JournalEntry.Create(
            tenantId,
            $"PR-{payroll.Id.Value.ToString().Substring(0,8)}",
            $"Payroll processing for {employee.FirstName} {employee.LastName}");

        var expenseAccId = new AccountId(request.PayrollExpenseAccountId);
        var liabilityAccId = new AccountId(request.PayrollLiabilityAccountId);

        var expenseAcc = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == expenseAccId, cancellationToken);
        var liabilityAcc = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == liabilityAccId, cancellationToken);

        if (expenseAcc == null || liabilityAcc == null)
            return Result.Failure<Guid>(new Error("Account.NotFound", "Payroll accounts not found.", ErrorType.NotFound));

        je.AddLine(expenseAccId, payroll.GrossSalary, 0, "Gross Salary Expense");
        je.AddLine(liabilityAccId, 0, payroll.NetSalary, "Net Salary Payable");
        if (payroll.TaxDeductions > 0)
        {
            // Just putting taxes into the same liability for simplicity, or we should have a separate tax account
            je.AddLine(liabilityAccId, 0, payroll.TaxDeductions, "Tax Deductions Payable");
        }

        expenseAcc.UpdateBalance(payroll.GrossSalary);
        liabilityAcc.UpdateBalance(payroll.GrossSalary); // Net + Tax

        je.Post();
        _context.JournalEntries.Add(je);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(payroll.Id.Value);
    }
}
