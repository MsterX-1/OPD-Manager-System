using System.Windows;
using Microsoft.Win32;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using OpdManager.Presentation.Helpers;

namespace OpdManager.Presentation.ViewModels;

public class ReportsViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IReportService _reportService;

    private DateTime _fromDate = DateTime.Today;
    private DateTime _toDate = DateTime.Today;
    private bool _isLoading;

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public RelayCommand ExportPdfCommand { get; }
    public RelayCommand ExportExcelCommand { get; }
    public RelayCommand PrintCommand { get; }

    public ReportsViewModel(
        IAppointmentService appointmentService,
        IReportService reportService)
    {
        _appointmentService = appointmentService;
        _reportService = reportService;

        ExportPdfCommand = new RelayCommand(async () => await ExportPdfAsync());
        ExportExcelCommand = new RelayCommand(async () => await ExportExcelAsync());
        PrintCommand = new RelayCommand(async () => await PrintAsync());
    }

    private async Task<List<Appointment>> GetAppointmentsAsync()
    {
        var result = new List<Appointment>();
        for (var date = FromDate.Date; date <= ToDate.Date; date = date.AddDays(1))
        {
            var daily = await _appointmentService.GetByDateAsync(date);
            result.AddRange(daily.Select(dto => new Appointment
            {
                Id = dto.Id,
                AppointmentDate = dto.AppointmentDate,
                TimeSlotTicks = dto.TimeSlot.Ticks,
                Status = dto.Status,
                Notes = dto.Notes,
                Doctor = new Doctor { Name = dto.DoctorName, Specialty = dto.DoctorSpecialty },
                Patient = new Patient { FullName = dto.PatientName }
            }));
        }
        return result;
    }

    private async Task ExportPdfAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "PDF Files|*.pdf",
            FileName = $"Appointments_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.pdf"
        };

        if (dlg.ShowDialog() != true) return;

        IsLoading = true;
        try
        {
            var data = await GetAppointmentsAsync();
            _reportService.ExportAppointmentsToPdf(data, dlg.FileName);
            MessageBox.Show("PDF exported successfully!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private async Task ExportExcelAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            FileName = $"Appointments_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.xlsx"
        };

        if (dlg.ShowDialog() != true) return;

        IsLoading = true;
        try
        {
            var data = await GetAppointmentsAsync();
            _reportService.ExportAppointmentsToExcel(data, dlg.FileName);
            MessageBox.Show("Excel exported successfully!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private async Task PrintAsync()
    {
        IsLoading = true;
        try
        {
            var data = await GetAppointmentsAsync();
            PrintHelper.PrintAppointments(data); // ← Presentation helper, not service
        }
        finally { IsLoading = false; }
    }
}