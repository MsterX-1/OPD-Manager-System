using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class AddEditAppointmentViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    // Form fields
    private int _id;
    private bool _isEditMode;
    private string _title = "New Appointment";
    private int _editPatientId;
    private int _editDoctorId;

    private Doctor? _selectedDoctor;
    private Patient? _selectedPatient;
    private DateTime _appointmentDate = DateTime.Today;
    private TimeSpan _selectedTimeSlot = new(8, 0, 0);
    private string _notes = string.Empty;

    // Patient quick search
    private string _patientSearch = string.Empty;
    private ObservableCollection<Patient> _patientResults = new();
    private bool _showPatientResults;

    // New patient quick-add
    private string _newPatientName = string.Empty;
    private string _newPatientPhone = string.Empty;
    private bool _showQuickAdd;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public Doctor? SelectedDoctor
    {
        get => _selectedDoctor;
        set { SetProperty(ref _selectedDoctor, value); SaveCommand.RaiseCanExecuteChanged(); }
    }

    public Patient? SelectedPatient
    {
        get => _selectedPatient;
        set { SetProperty(ref _selectedPatient, value); SaveCommand.RaiseCanExecuteChanged(); }
    }

    public DateTime AppointmentDate
    {
        get => _appointmentDate;
        set => SetProperty(ref _appointmentDate, value);
    }

    public TimeSpan SelectedTimeSlot
    {
        get => _selectedTimeSlot;
        set => SetProperty(ref _selectedTimeSlot, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public string PatientSearch
    {
        get => _patientSearch;
        set
        {
            SetProperty(ref _patientSearch, value);
            // Clear selected patient when user types new text
            if (SelectedPatient != null && SelectedPatient.FullName != value)
            {
                SelectedPatient = null;
                SaveCommand.RaiseCanExecuteChanged();
            }
            _ = SearchPatientsAsync();
        }
    }

    public ObservableCollection<Patient> PatientResults
    {
        get => _patientResults;
        set => SetProperty(ref _patientResults, value);
    }

    public bool ShowPatientResults
    {
        get => _showPatientResults;
        set => SetProperty(ref _showPatientResults, value);
    }

    public string NewPatientName
    {
        get => _newPatientName;
        set => SetProperty(ref _newPatientName, value);
    }

    public string NewPatientPhone
    {
        get => _newPatientPhone;
        set => SetProperty(ref _newPatientPhone, value);
    }

    public bool ShowQuickAdd
    {
        get => _showQuickAdd;
        set => SetProperty(ref _showQuickAdd, value);
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();
    public ObservableCollection<TimeSpan> TimeSlots { get; } = new();

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand ToggleQuickAddCommand { get; }
    public RelayCommand<Patient> SelectPatientCommand { get; }
    public RelayCommand QuickAddPatientCommand { get; }

    public event Action? OnSaved;
    public event Action? OnCancelled;

    public AddEditAppointmentViewModel(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;

        SaveCommand = new RelayCommand(
            async () => await SaveAsync(),
            () => SelectedDoctor != null && SelectedPatient != null);

        CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
        ToggleQuickAddCommand = new RelayCommand(() => ShowQuickAdd = !ShowQuickAdd);
        SelectPatientCommand = new RelayCommand<Patient>(SelectPatient);
        QuickAddPatientCommand = new RelayCommand(
            async () => await QuickAddPatientAsync(),
            () => !string.IsNullOrWhiteSpace(NewPatientName));

        GenerateTimeSlots();
    }

    public async Task InitializeAsync()
    {
        var doctors = await _doctorService.GetAllAsync();
        Doctors.Clear();
        foreach (var d in doctors.Where(d => d.IsActive))
            Doctors.Add(d);

        // If editing, restore SelectedDoctor and SelectedPatient after doctors loaded
        if (_isEditMode)
        {
            SelectedDoctor = Doctors.FirstOrDefault(d => d.Id == _editDoctorId);

            // Load patient by id to get the actual Patient object
            if (_editPatientId > 0)
            {
                var patient = await _patientService.GetByIdAsync(_editPatientId);
                if (patient != null)
                {
                    SelectedPatient = patient;
                    PatientSearch = patient.FullName;
                }
            }
        }
    }

    public void LoadAppointment(AppointmentDto dto)
    {
        _isEditMode = true;
        Title = "Edit Appointment";
        _id = dto.Id;
        _editPatientId = dto.PatientId;
        _editDoctorId = dto.DoctorId;
        AppointmentDate = dto.AppointmentDate;
        SelectedTimeSlot = dto.TimeSlot;
        Notes = dto.Notes ?? string.Empty;
        // Doctor and Patient restored in InitializeAsync after collections load
    }

    private void GenerateTimeSlots()
    {
        for (int h = 0; h < 24; h++)
            for (int m = 0; m < 60; m += 30)
                TimeSlots.Add(new TimeSpan(h, m, 0));
    }

    private async Task SearchPatientsAsync()
    {
        if (string.IsNullOrWhiteSpace(PatientSearch))
        {
            ShowPatientResults = false;
            PatientResults.Clear();
            return;
        }

        var results = await _patientService.SearchAsync(PatientSearch);
        PatientResults = new ObservableCollection<Patient>(results);
        ShowPatientResults = PatientResults.Any();
    }

    private void SelectPatient(Patient patient)
    {
        SelectedPatient = patient;
        PatientSearch = patient.FullName;
        ShowPatientResults = false;
    }

    private async Task QuickAddPatientAsync()
    {
        var patient = new Patient
        {
            FullName = NewPatientName,
            PhoneNumber = NewPatientPhone,
            CreatedAt = DateTime.Now
        };

        await _patientService.AddAsync(patient);

        SelectedPatient = patient;
        PatientSearch = patient.FullName;
        ShowQuickAdd = false;
        NewPatientName = string.Empty;
        NewPatientPhone = string.Empty;

        MessageBox.Show($"Patient {patient.FullName} created!",
            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SaveAsync()
    {
        if (SelectedDoctor is null)
        {
            MessageBox.Show("Please select a doctor.", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedPatient is null)
        {
            MessageBox.Show("Please select a patient from the search results or create a new one.",
                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var appointment = new Appointment
        {
            Id = _id,
            DoctorId = SelectedDoctor.Id,
            PatientId = SelectedPatient.Id,
            AppointmentDate = AppointmentDate,
            TimeSlot = SelectedTimeSlot,
            Notes = Notes
        };

        var (success, error) = await _appointmentService.BookAsync(appointment);

        if (!success)
        {
            MessageBox.Show(error, "Booking Failed",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OnSaved?.Invoke();
    }
}