using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Application.Services.Implementations;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Infrastructure.Data;
using OpdManager.Infrastructure.Repositories;
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

        // Repositories — scoped so they share the same DbContext per scope
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<IQueueRepository, QueueRepository>();

        // Unit of Work — gets repositories injected automatically
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IIssueService, IssueService>();

        // ViewModels
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<DoctorViewModel>();
        services.AddScoped<AppointmentViewModel>();
        services.AddScoped<QueueMonitorViewModel>();
        services.AddScoped<IssueViewModel>();
        services.AddTransient<AddEditDoctorViewModel>();
        services.AddTransient<AddEditAppointmentViewModel>();
        services.AddTransient<AddEditIssueViewModel>();

        // MainViewModel — manual factory to inject IServiceProvider
        services.AddScoped<MainViewModel>(provider => new MainViewModel(
            provider.GetRequiredService<DashboardViewModel>(),
            provider.GetRequiredService<DoctorViewModel>(),
            provider.GetRequiredService<AppointmentViewModel>(),
            provider.GetRequiredService<QueueMonitorViewModel>(),
            provider.GetRequiredService<IssueViewModel>(),
            provider
        ));

        // Windows
        services.AddTransient<MainWindow>();
    }
}

