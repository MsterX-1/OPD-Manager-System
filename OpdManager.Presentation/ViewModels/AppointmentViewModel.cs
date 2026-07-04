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

    private const int PageSize = 20;

    public static readonly Doctor AllDoctors = new() { Id = -1, Name = "All Doctors" };

    // --- Collections ---
    private ObservableCollection<AppointmentDto> _appointments = new();
    private ObservableCollection<Doctor> _doctors = new();

    // --- Selection ---
    private AppointmentDto? _selectedAppointment;
    private Doctor _filterDoctor = AllDoctors;

    // --- Filter mode ---
    private bool _isSearchMode = false; // false = date filter, true = search mode
    private DateTime _selectedDate = DateTime.Today;
    private DateTime? _searchFromDate;
    private DateTime? _searchToDate;
    private string _searchText = string.Empty;

    // --- Pagination ---
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;

    // --- State ---
    private bool _isLoading;
    private bool _doctorsLoaded;
    private bool _suppressFilterReload;
    private string _statusText = string.Empty;

    public ObservableCollection<AppointmentDto> Appointments
    {
        get => _appointments;
        set => SetProperty(ref _appointments, value);
    }

    public ObservableCollection<Doctor> Doctors
    {
        get => _doctors;
        set => SetProperty(ref _doctors, value);
    }

    public AppointmentDto? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            SetProperty(ref _selectedAppointment, value);
            EditCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            CheckInCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }

    public Doctor FilterDoctor
    {
        get => _filterDoctor;
        set
        {
            SetProperty(ref _filterDoctor, value ?? AllDoctors);
            if (!_suppressFilterReload)
                _ = LoadCurrentModeAsync();
        }
    }

    public bool IsSearchMode
    {
        get => _isSearchMode;
        set => SetProperty(ref _isSearchMode, value);
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            SetProperty(ref _selectedDate, value);
            if (!_suppressFilterReload && !_isSearchMode)
                _ = LoadDateModeAsync(1);
        }
    }

    public DateTime? SearchFromDate
    {
        get => _searchFromDate;
        set => SetProperty(ref _searchFromDate, value);
    }

    public DateTime? SearchToDate
    {
        get => _searchToDate;
        set => SetProperty(ref _searchToDate, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
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

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    // Commands
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand CheckInCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand SearchCommand { get; }
    public RelayCommand ClearSearchCommand { get; }
    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand NextPageCommand { get; }

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
                            () => SelectedAppointment?.Status == AppointmentStatus.Scheduled);
        CancelCommand = new RelayCommand(async () => await CancelAppointmentAsync(),
                            () => SelectedAppointment?.Status == AppointmentStatus.Scheduled);
        CheckInCommand = new RelayCommand(async () => await CheckInAsync(),
                            () => SelectedAppointment?.Status == AppointmentStatus.Scheduled &&
                                  SelectedAppointment?.QueueNumber == null);
        DeleteCommand = new RelayCommand(async () => await DeleteAppointmentAsync(),
                            () => SelectedAppointment != null);
        RefreshCommand = new RelayCommand(async () => await LoadCurrentModeAsync());
        TodayCommand = new RelayCommand(() =>
        {
            IsSearchMode = false;
            SelectedDate = DateTime.Today;
        });

        SearchCommand = new RelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(SearchText) &&
                SearchFromDate == null && SearchToDate == null)
            {
                MessageBox.Show("Enter a patient name or date range to search.",
                    "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            IsSearchMode = true;
            await LoadSearchModeAsync(1);
        });

        ClearSearchCommand = new RelayCommand(async () =>
        {
            SearchText = string.Empty;
            SearchFromDate = null;
            SearchToDate = null;
            IsSearchMode = false;
            await LoadDateModeAsync(1);
        });

        PreviousPageCommand = new RelayCommand(async () =>
        {
            if (CanGoPrevious)
                await LoadCurrentModeAsync(CurrentPage - 1);
        });

        NextPageCommand = new RelayCommand(async () =>
        {
            if (CanGoNext)
                await LoadCurrentModeAsync(CurrentPage + 1);
        });
    }

    public void ResetDoctorsCache() => _doctorsLoaded = false;

    // Entry point — called on nav and refresh
    public async Task LoadAsync() => await LoadCurrentModeAsync(1);

    private async Task LoadCurrentModeAsync(int page = 0)
    {
        var targetPage = page > 0 ? page : CurrentPage;
        if (_isSearchMode)
            await LoadSearchModeAsync(targetPage);
        else
            await LoadDateModeAsync(targetPage);
    }

    private async Task LoadDateModeAsync(int page)
    {
        IsLoading = true;
        try
        {
            await EnsureDoctorsLoadedAsync();

            var doctorId = FilterDoctor.Id > 0 ? FilterDoctor.Id : 0;
            var paged = await _appointmentService
                .GetByDatePagedAsync(SelectedDate, page, PageSize, doctorId);

            ApplyPagedResult(paged, page);
            StatusText = $"Showing appointments for {SelectedDate:MMM dd, yyyy}";
        }
        finally { IsLoading = false; }
    }

    private async Task LoadSearchModeAsync(int page)
    {
        IsLoading = true;
        try
        {
            await EnsureDoctorsLoadedAsync();

            var doctorId = FilterDoctor.Id > 0 ? FilterDoctor.Id : 0;
            var paged = await _appointmentService.SearchPagedAsync(
                string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                doctorId, SearchFromDate, SearchToDate, page, PageSize);

            ApplyPagedResult(paged, page);
            StatusText = $"Search results for \"{SearchText}\"";
        }
        finally { IsLoading = false; }
    }

    private void ApplyPagedResult(Application.DTOs.PagedResult<AppointmentDto> paged, int page)
    {
        Appointments = new ObservableCollection<AppointmentDto>(paged.Items);
        CurrentPage = page;
        TotalPages = paged.TotalPages == 0 ? 1 : paged.TotalPages;
        TotalCount = paged.TotalCount;

        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private async Task EnsureDoctorsLoadedAsync()
    {
        if (_doctorsLoaded) return;

        var doctors = await _doctorService.GetAllAsync();
        var currentId = FilterDoctor.Id;

        _suppressFilterReload = true;
        Doctors = new ObservableCollection<Doctor> { AllDoctors };
        foreach (var d in doctors) Doctors.Add(d);

        FilterDoctor = currentId == -1
            ? AllDoctors
            : Doctors.FirstOrDefault(d => d.Id == currentId) ?? AllDoctors;

        _suppressFilterReload = false;
        _doctorsLoaded = true;
    }

    private async Task CancelAppointmentAsync()
    {
        if (SelectedAppointment is null) return;
        var name = SelectedAppointment.PatientName;
        var id = SelectedAppointment.Id;

        var result = MessageBox.Show($"Cancel appointment for {name}?",
            "Confirm Cancel", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        await _appointmentService.CancelAsync(id);
        await LoadCurrentModeAsync();
    }

    private async Task DeleteAppointmentAsync()
    {
        if (SelectedAppointment is null) return;
        var name = SelectedAppointment.PatientName;
        var id = SelectedAppointment.Id;

        var result = MessageBox.Show(
            $"Permanently delete appointment for {name}?\n\nThis cannot be undone.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        await _appointmentService.DeleteAsync(id);
        await LoadCurrentModeAsync();
    }

    private async Task CheckInAsync()
    {
        if (SelectedAppointment is null) return;
        var name = SelectedAppointment.PatientName;
        var id = SelectedAppointment.Id;

        try
        {
            await _queueService.CheckInAsync(id);
            await LoadCurrentModeAsync();
            MessageBox.Show($"{name} checked in successfully!",
                "Check In", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Check-in failed: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}