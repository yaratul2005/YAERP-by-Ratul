using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAERP.Application.Common.Interfaces;
using YAERP.Domain.Entities.Hcm;

namespace YAERP.Infrastructure.Hcm;

public class AttendanceProcessingService : IAttendanceProcessingService
{
    private readonly IApplicationDbContext _dbContext;

    public AttendanceProcessingService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BiometricLogEntry>> ParseBiometricLogsAsync(Stream logStream, CancellationToken cancellationToken = default)
    {
        var entries = new List<BiometricLogEntry>();
        using var reader = new StreamReader(logStream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Simple CSV parser: EmployeeCode,Timestamp,Action
            var parts = line.Split(',');
            if (parts.Length == 3)
            {
                if (DateTime.TryParse(parts[1], out var timestamp))
                {
                    entries.Add(new BiometricLogEntry(parts[0].Trim(), timestamp, parts[2].Trim().ToUpper()));
                }
            }
        }

        return entries.OrderBy(e => e.TimestampUtc).ToList();
    }

    public async Task ProcessAttendanceAsync(List<BiometricLogEntry> logs, CancellationToken cancellationToken = default)
    {
        var empCodes = logs.Select(l => l.EmployeeCode).Distinct().ToList();
        var employees = await _dbContext.Set<Employee>()
            .Where(e => empCodes.Contains(e.EmployeeCode))
            .ToDictionaryAsync(e => e.EmployeeCode, cancellationToken);

        var groupedLogs = logs.GroupBy(l => l.EmployeeCode);

        foreach (var empLogs in groupedLogs)
        {
            if (!employees.TryGetValue(empLogs.Key, out var employee)) continue;

            DateTime? currentIn = null;

            foreach (var log in empLogs.OrderBy(l => l.TimestampUtc))
            {
                if (log.ActionType == "IN" && currentIn == null)
                {
                    currentIn = log.TimestampUtc;
                }
                else if (log.ActionType == "OUT" && currentIn != null)
                {
                    var clockOut = log.TimestampUtc;
                    var duration = (clockOut - currentIn.Value).TotalHours;

                    decimal regularHours = 0m;
                    decimal overtimeHours = 0m;

                    if (duration > 8)
                    {
                        regularHours = 8m;
                        overtimeHours = (decimal)(duration - 8);
                    }
                    else
                    {
                        regularHours = (decimal)duration;
                    }

                    var record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = employee.Id,
                        ClockInUtc = currentIn.Value,
                        ClockOutUtc = clockOut,
                        RegularHours = regularHours,
                        OvertimeHours = overtimeHours,
                        Status = overtimeHours > 0 ? "Overtime" : "Present"
                    };

                    _dbContext.Set<AttendanceRecord>().Add(record);

                    currentIn = null;
                }
            }
        }
    }
}
