using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class PatientViewModel : BaseViewModel
{
    private readonly IPatientService _patientService;

    private const int PageSize = 20;

    private ObservableCollection<Patient> _patients = new();
    private Patient? _selectedPatient;
    private string _searchText = string.Empty;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private bool _isLoading;
    private string _statusText = "All patients";

    public ObservableCollection<Patient> Patients
    {
        get => _patients;
        set => SetProperty(ref _patients, value);
    }

    public Patient? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            SetProperty(ref _selectedPatient, value);
            ViewHistoryCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
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

    public RelayCommand SearchCommand       { get; }
    public RelayCommand ClearSearchCommand  { get; }
    public RelayCommand ViewHistoryCommand  { get; }
    public RelayCommand CreatePatientCommand { get; }
    public RelayCommand DeleteCommand       { get; }
    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand NextPageCommand     { get; }

    public event Action<Patient>? OpenPatientHistory;
    public event Action? OpenCreatePatient;

    public PatientViewModel(IPatientService patientService)
    {
        _patientService = patientService;

        SearchCommand = new RelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Length < 2)
            {
                MessageBox.Show("Enter at least 2 characters to search.",
                    "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await LoadAsync(1, SearchText);
        });

        ClearSearchCommand = new RelayCommand(async () =>
        {
            SearchText = string.Empty;
            await LoadAsync(1);
        });

        CreatePatientCommand = new RelayCommand(() => OpenCreatePatient?.Invoke());

        ViewHistoryCommand = new RelayCommand(
            () => { if (SelectedPatient != null) OpenPatientHistory?.Invoke(SelectedPatient); },
            () => SelectedPatient != null);

        DeleteCommand = new RelayCommand(async () => await DeleteAsync(),
            () => SelectedPatient != null);

        PreviousPageCommand = new RelayCommand(async () =>
        {
            if (CanGoPrevious)
                await LoadAsync(CurrentPage - 1, SearchText);
        });

        NextPageCommand = new RelayCommand(async () =>
        {
            if (CanGoNext)
                await LoadAsync(CurrentPage + 1, SearchText);
        });
    }

    public async Task LoadAsync(int page = 1, string? search = null)
    {
        IsLoading = true;
        try
        {
            IEnumerable<Patient> patients;

            if (!string.IsNullOrWhiteSpace(search))
            {
                patients = await _patientService.SearchAsync(search);
                StatusText = $"Results for \"{search}\"";
            }
            else
            {
                patients = await _patientService.GetAllAsync();
                StatusText = "All patients";
            }

            var list = patients.ToList();
            TotalCount = list.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            TotalPages = TotalPages == 0 ? 1 : TotalPages;
            CurrentPage = page;

            var paged = list
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Patients = new ObservableCollection<Patient>(paged);

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
        finally { IsLoading = false; }
    }

    private async Task DeleteAsync()
    {
        if (SelectedPatient is null) return;

        var result = MessageBox.Show(
            $"Delete patient \"{SelectedPatient.FullName}\"?\n\nThis will also delete all their appointments.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        await _patientService.DeleteAsync(SelectedPatient.Id);
        await LoadAsync(1, SearchText);
    }
}