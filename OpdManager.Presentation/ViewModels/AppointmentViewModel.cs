using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class AppointmentViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IQueueService _queueService;

    private ObservableCollection<AppointmentDto> _appointments = new();
    private AppointmentDto? _selectedAppointment;
    private DateTime _selectedDate = DateTime.Today;
    private ObservableCollection<Doctor?> _doctors = new();
    private Doctor? _filterDoctor;
    private bool _isLoading;
    private bool _doctorsLoaded = false;
    private bool _suppressFilterReload = false; // ← prevents reentrance

    public ObservableCollection<AppointmentDto> Appointments
    {
        get => _appointments;
        set => SetProperty(ref _appointments, value);
    }

    public AppointmentDto? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            SetProperty(ref _selectedAppointment, value);
            CancelCommand.RaiseCanExecuteChanged();
            CheckInCommand.RaiseCanExecuteChanged();
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            SetProperty(ref _selectedDate, value);
            if (!_suppressFilterReload)
                _ = LoadAsync();
        }
    }

    public ObservableCollection<Doctor?> Doctors
    {
        get => _doctors;
        set => SetProperty(ref _doctors, value);
    }

    public Doctor? FilterDoctor
    {
        get => _filterDoctor;
        set
        {
            SetProperty(ref _filterDoctor, value);
            if (!_suppressFilterReload) // ← only reload if not restoring selection
                _ = LoadAsync();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand CheckInCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public event Action<AppointmentDto?>? OpenAppointmentDialog;

    public AppointmentViewModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IQueueService queueService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _queueService = queueService;

        AddCommand = new RelayCommand(() => OpenAppointmentDialog?.Invoke(null));
        EditCommand = new RelayCommand(() => OpenAppointmentDialog?.Invoke(SelectedAppointment),
                                          () => SelectedAppointment != null &&
                                                SelectedAppointment.Status == AppointmentStatus.Scheduled);
        CancelCommand = new RelayCommand(async () => await CancelAppointmentAsync(),
                                          () => SelectedAppointment != null &&
                                                SelectedAppointment.Status == AppointmentStatus.Scheduled);
        CheckInCommand = new RelayCommand(async () => await CheckInAsync(),
                                          () => SelectedAppointment != null &&
                                                SelectedAppointment.Status == AppointmentStatus.Scheduled &&
                                                SelectedAppointment.QueueNumber == null);
        RefreshCommand = new RelayCommand(async () => await LoadAsync());
        TodayCommand = new RelayCommand(() => SelectedDate = DateTime.Today);
        DeleteCommand = new RelayCommand(async () => await DeleteAppointmentAsync(),
                                          () => SelectedAppointment != null);
    }

    public void ResetDoctorsCache() => _doctorsLoaded = false;

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            if (!_doctorsLoaded)
            {
                var doctors = await _doctorService.GetAllAsync();
                var currentId = FilterDoctor?.Id;

                // Suppress reload while we restore the selection
                _suppressFilterReload = true;

                Doctors = new ObservableCollection<Doctor?> { null };
                foreach (var d in doctors)
                    Doctors.Add(d);

                // Restore previous selection without triggering LoadAsync again
                FilterDoctor = currentId != null
                    ? Doctors.FirstOrDefault(d => d?.Id == currentId)
                    : null;

                _suppressFilterReload = false;
                _doctorsLoaded = true;
            }

            var appointments = await _appointmentService.GetByDateAsync(SelectedDate);

            if (FilterDoctor != null)
                appointments = appointments
                    .Where(a => a.DoctorName == FilterDoctor.Name);

            Appointments = new ObservableCollection<AppointmentDto>(appointments);
        }
        finally
        {
            IsLoading = false;
            _suppressFilterReload = false; // safety reset
        }
    }

    private async Task DeleteAppointmentAsync()
    {
        if (SelectedAppointment is null) return;

        var result = MessageBox.Show(
            $"Permanently delete appointment for {SelectedAppointment.PatientName}?\n\nThis cannot be undone.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        await _appointmentService.DeleteAsync(SelectedAppointment.Id);
        await LoadAsync();
    }

    private async Task CancelAppointmentAsync()
    {
        if (SelectedAppointment is null) return;

        var result = MessageBox.Show(
            $"Cancel appointment for {SelectedAppointment.PatientName}?",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        await _appointmentService.CancelAsync(SelectedAppointment.Id);
        await LoadAsync();
    }

    private async Task CheckInAsync()
    {
        if (SelectedAppointment is null) return;

        var patientName = SelectedAppointment.PatientName;
        var appointmentId = SelectedAppointment.Id;

        try
        {
            await _queueService.CheckInAsync(appointmentId);
            await LoadAsync();
            MessageBox.Show(
                $"{patientName} checked in successfully!",
                "Check In", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Check-in failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}