using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Application.Common.Interfaces;

public interface IFixedAssetDepreciationService
{
    Task<List<DepreciationScheduleEntry>> GenerateScheduleAsync(Guid fixedAssetId, CancellationToken cancellationToken = default);
    Task PostDepreciationEntriesAsync(int periodYear, int periodMonth, CancellationToken cancellationToken = default);
}
