using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using YAERP.Application;
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
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
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

    public async Task StartAsync()
    {
        await AppHost!.StartAsync();
    }

    public async Task StopAsync()
    {
        await AppHost!.StopAsync();
    }
}
