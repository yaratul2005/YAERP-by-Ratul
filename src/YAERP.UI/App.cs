using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using YAERP.Application;
using YAERP.Application.Common.Interfaces;
using YAERP.Infrastructure;
using YAERP.Infrastructure.Hardware;
using YAERP.UI.Services;
using YAERP.UI.ViewModels;
using YAERP.UI.ViewModels.Security;
using YAERP.UI.Views;

namespace YAERP.UI;

public partial class App : System.Windows.Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        InitializeComponent();

        // Prevent WPF from shutting down during async OnStartup before MainWindow is shown
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Global domain exception handling
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "A fatal unhandled domain exception occurred.");
            Log.CloseAndFlush();
        };

        // Dispatcher unhandled exception handling to catch UI thread exceptions and prevent silent app crashes
        DispatcherUnhandledException += OnDispatcherUnhandledException;

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
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ITenantContext, MockTenantContext>();

                    // Register Layers
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);

                    // Register Hardware Helpers
                    services.AddSingleton<BarcodeScannerListener>();

                    // Register Navigation, Workspace, Command Palette, Dialog Service, Modal Service & Main Window
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddSingleton<IModalService, ModalService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<ITabWorkspaceService, TabWorkspaceService>();
                    services.AddSingleton<ICommandPaletteService, CommandPaletteService>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<MainWindow>();

                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<InventoryViewModel>();
                    services.AddTransient<SalesViewModel>();
                    services.AddTransient<FinanceViewModel>();
                    services.AddTransient<PosViewModel>();
                    services.AddTransient<ApprovalCenterViewModel>();
                    services.AddTransient<PluginHubViewModel>();
                    services.AddTransient<ManufacturingViewModel>();
                    services.AddTransient<WarehouseManagementViewModel>();
                    services.AddTransient<DeepFinancialsViewModel>();
                    services.AddTransient<UserAndRolesViewModel>();
                    services.AddTransient<SystemSettingsViewModel>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "An unhandled UI Dispatcher exception occurred.");
        MessageBox.Show($"UI Operation Error: {e.Exception.Message}", "YAERP Navigation Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            await StartAsync();

            var mainWindow = AppHost!.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.MainWindow = mainWindow;

            // Re-enable automatic shutdown when main window closes
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application Startup Failed");
            MessageBox.Show($"Application Startup Failed: {ex.Message}", "YAERP Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

public class MockTenantContext : ITenantContext
{
    public YAERP.Domain.Identity.TenantId? CurrentTenantId => null;
    public YAERP.Domain.Identity.UserId? CurrentUserId => null;
}
