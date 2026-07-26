using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;
using YAERP.Infrastructure.Reporting;

namespace YAERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveInterceptor>();

        // Ensure to fallback to in memory or sqlite if no connection string is found for testing.
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<YAERPDbContext>((sp, options) =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("YAERP_Db");
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<YAERPDbContext>());
        services.AddScoped<IDatabaseSeeder, YAERPDbContextSeeder>();

        services.AddSingleton<IPdfReportGenerator, QuestPdfReportGenerator>();
        services.AddSingleton<IExcelExporter, ClosedXmlExporter>();

        return services;
    }
}
