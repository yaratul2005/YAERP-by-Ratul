using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;
using YAERP.Infrastructure.Reporting;
using YAERP.Infrastructure.AI.Services;

namespace YAERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<YAERPDbContext>((sp, options) =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appDataPath, "YAERP");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                var dbPath = Path.Combine(appFolder, "yaerp_local.db");
                options.UseSqlite($"Data Source={dbPath}");
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<YAERPDbContext>());
        services.AddScoped<IDatabaseSeeder, YAERPDbContextSeeder>();

        services.AddSingleton<IPdfReportGenerator, QuestPdfReportGenerator>();
        services.AddSingleton<IExcelExporter, ClosedXmlExporter>();

        services.AddScoped<IInventoryForecastingService, InventoryForecastingService>();

        return services;
    }
}
