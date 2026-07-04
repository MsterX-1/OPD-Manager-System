using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Application.Services.Implementations;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Infrastructure.Data;
using OpdManager.Infrastructure.Repositories;
using OpdManager.Infrastructure.Services;
using OpdManager.Infrastructure.UnitOfWork;
using OpdManager.Presentation.Helpers;
using OpdManager.Presentation.ViewModels;

namespace OpdManager.Presentation;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handlers
        DispatcherUnhandledException            += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException   += OnUnobservedTaskException;

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        Logger.Info("Application started.");

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.Info("Application exited.");
        base.OnExit(e);
    }

    // UI thread exceptions
    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Error("Unhandled UI exception", e.Exception);
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe error has been logged. The app will continue running.",
            "Unexpected Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true; // prevent crash
    }

    // Background thread exceptions
    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Logger.Error("Unhandled domain exception", ex);
    }

    // Async task exceptions
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.Error("Unobserved task exception", e.Exception);
        e.SetObserved();
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
        services.AddScoped<IReportService, ReportService>();

        // ViewModels
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<DoctorViewModel>();
        services.AddScoped<AppointmentViewModel>();
        services.AddScoped<QueueMonitorViewModel>();
        services.AddScoped<IssueViewModel>();
        services.AddScoped<ReportsViewModel>(provider => new ReportsViewModel(
            provider.GetRequiredService<IAppointmentService>(),
            provider.GetRequiredService<IReportService>()));
        services.AddScoped<PatientViewModel>();

        services.AddTransient<PatientHistoryViewModel>();
        services.AddTransient<CreatePatientViewModel>();
        services.AddTransient<SettingsViewModel>();
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
            provider.GetRequiredService<ReportsViewModel>(),
            provider.GetRequiredService<PatientViewModel>(),
            provider
        ));

        // Windows
        services.AddTransient<MainWindow>();
    }
}

