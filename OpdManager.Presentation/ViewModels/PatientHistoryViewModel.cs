using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;

namespace OpdManager.Presentation.ViewModels;

public class PatientHistoryViewModel : BaseViewModel
{
    private readonly IAppointmentService _appointmentService;

    private const int PageSize = 20;

    private Patient? _patient;
    private ObservableCollection<AppointmentDto> _appointments = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private bool _isLoading;

    public Patient? Patient
    {
        get => _patient;
        set => SetProperty(ref _patient, value);
    }

    public ObservableCollection<AppointmentDto> Appointments
    {
        get => _appointments;
        set => SetProperty(ref _appointments, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand NextPageCommand { get; }

    public PatientHistoryViewModel(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;

        PreviousPageCommand = new RelayCommand(async () =>
        {
            if (CanGoPrevious && _patient != null)
                await LoadAsync(_patient, CurrentPage - 1);
        });

        NextPageCommand = new RelayCommand(async () =>
        {
            if (CanGoNext && _patient != null)
                await LoadAsync(_patient, CurrentPage + 1);
        });
    }

    public async Task LoadAsync(Patient patient, int page = 1)
    {
        Patient = patient;
        IsLoading = true;
        try
        {
            var paged = await _appointmentService
                .GetByPatientPagedAsync(patient.Id, page, PageSize);

            Appointments = new ObservableCollection<AppointmentDto>(paged.Items);
            CurrentPage = page;
            TotalPages = paged.TotalPages == 0 ? 1 : paged.TotalPages;
            TotalCount = paged.TotalCount;

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
        finally { IsLoading = false; }
    }
}