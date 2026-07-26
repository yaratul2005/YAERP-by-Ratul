using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using YAERP.Application;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure;
using YAERP.UI.Services;
using YAERP.UI.ViewModels;

namespace YAERP.UI;

// Simulating App.xaml.cs for dependency injection setup without WPF SDK
public class App
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        // Global exception handling
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "A fatal unhandled exception occurred.");
            Log.CloseAndFlush();
        };

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/yaerp-.log", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("Starting application host...");

            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog() // Wire up Serilog into Microsoft.Extensions.Logging
                .ConfigureServices((context, services) =>
                {
                    // For mock tenant logic in a desktop app, we can register a fake one if needed
                    services.AddSingleton<ITenantContext, MockTenantContext>();

                    // Register Layers
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);

                    // Register Navigation
                    services.AddSingleton<INavigationService, NavigationService>();

                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<InventoryViewModel>();
                    services.AddTransient<SalesViewModel>();
                    services.AddTransient<FinanceViewModel>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using (var scope = AppHost!.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }

        await AppHost!.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await AppHost!.StopAsync(cancellationToken);
        Log.CloseAndFlush();
    }
}

// Temporary to compile
public class MockTenantContext : ITenantContext
{
    public YAERP.Domain.Identity.TenantId? CurrentTenantId => null; // Mock
    public YAERP.Domain.Identity.UserId? CurrentUserId => null;
}
