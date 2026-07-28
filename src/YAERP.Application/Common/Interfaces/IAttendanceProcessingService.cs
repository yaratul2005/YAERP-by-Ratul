using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record BiometricLogEntry(string EmployeeCode, DateTime TimestampUtc, string ActionType);

public interface IAttendanceProcessingService
{
    Task<List<BiometricLogEntry>> ParseBiometricLogsAsync(Stream logStream, CancellationToken cancellationToken = default);
    Task ProcessAttendanceAsync(List<BiometricLogEntry> logs, CancellationToken cancellationToken = default);
}
