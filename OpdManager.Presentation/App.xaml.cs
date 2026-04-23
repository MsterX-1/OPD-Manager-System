using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Application.Interfaces;
using OpdManager.Infrastructure.Data;
using OpdManager.Infrastructure.UnitOfWork;
using OpdManager.Presentation.ViewModels;

namespace OpdManager.Presentation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Ensure DB is created
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=opd.db"));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ViewModels
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<MainViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }
}

