using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YAERP.Application.Common.Interfaces;
using YAERP.Application.Common.Interfaces.Reporting;
using YAERP.Application.Common.Interfaces.AI;
using YAERP.Application.Common.Plugins;
using YAERP.Application.Common.Security;
using YAERP.Infrastructure.Persistence;
using YAERP.Infrastructure.Persistence.Interceptors;
using YAERP.Infrastructure.Reporting;
using YAERP.Infrastructure.AI.Services;
using YAERP.Infrastructure.Hardware.Services;
using YAERP.Infrastructure.Plugins;
using YAERP.Infrastructure.Security;
using YAERP.Infrastructure.Sync.Options;
using YAERP.Infrastructure.Sync.Services;
using YAERP.Infrastructure.Sync.Workers;
using YAERP.Infrastructure.Workflows;

namespace YAERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveInterceptor>();
        services.AddScoped<SyncOutboxInterceptor>();

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
        services.AddSingleton<IReceiptPrinterService, ReceiptPrinterService>();
        services.AddSingleton<IHmacValidator, HmacValidator>();
        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<IIdentityService, IdentityService>();

        services.AddScoped<IInventoryForecastingService, InventoryForecastingService>();
        services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

        // Cloud Sync Services & Options
        services.Configure<CloudSyncOptions>(options =>
        {
            var section = configuration.GetSection(CloudSyncOptions.SectionName);
            if (section.Exists())
            {
                section.Bind(options);
            }
        });

        services.AddScoped<YAERP.Infrastructure.Sync.Protos.CloudSyncService.CloudSyncServiceClient>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CloudSyncOptions>>().Value;
            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(options.CloudServerUrl);
            return new YAERP.Infrastructure.Sync.Protos.CloudSyncService.CloudSyncServiceClient(channel);
        });

        services.AddScoped<ICloudSyncService, CloudSyncService>();
        services.AddHostedService<CloudSyncBackgroundWorker>();

        services.AddScoped<IBomExplosionService, YAERP.Infrastructure.Manufacturing.BomExplosionService>();
        services.AddScoped<IMrpExplosionService, YAERP.Infrastructure.Manufacturing.MrpExplosionService>();
        services.AddScoped<ISlottingOptimizationService, YAERP.Infrastructure.Warehouse.SlottingOptimizationService>();
        services.AddScoped<IFefoPickingService, YAERP.Infrastructure.Warehouse.FefoPickingService>();
        services.AddScoped<IInventoryValuationService, YAERP.Infrastructure.Warehouse.InventoryValuationService>();
        services.AddScoped<IFixedAssetDepreciationService, YAERP.Infrastructure.Financials.FixedAssetDepreciationService>();
        services.AddScoped<IForexRevaluationService, YAERP.Infrastructure.Financials.ForexRevaluationService>();
        services.AddScoped<ITaxComputationService, YAERP.Infrastructure.Financials.TaxComputationService>();

        return services;
    }
}
