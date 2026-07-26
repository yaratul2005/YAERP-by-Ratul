using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
