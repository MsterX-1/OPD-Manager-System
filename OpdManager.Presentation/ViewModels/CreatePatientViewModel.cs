using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels;

public class CreatePatientViewModel : BaseViewModel
{
    private readonly IPatientService _patientService;

    private string _fullName = string.Empty;
    private string _phoneNumber = string.Empty;
    private DateTime _dateOfBirth = DateTime.Today.AddYears(-30);
    private string _notes = string.Empty;

    public string FullName
    {
        get => _fullName;
        set { SetProperty(ref _fullName, value); SaveCommand.RaiseCanExecuteChanged(); }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public RelayCommand SaveCommand   { get; }
    public RelayCommand CancelCommand { get; }

    public event Action? OnSaved;
    public event Action? OnCancelled;

    public CreatePatientViewModel(IPatientService patientService)
    {
        _patientService = patientService;

        SaveCommand = new RelayCommand(
            async () => await SaveAsync(),
            () => !string.IsNullOrWhiteSpace(FullName));

        CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
    }

    private async Task SaveAsync()
    {
        await _patientService.AddAsync(new Patient
        {
            FullName    = FullName,
            PhoneNumber = PhoneNumber,
            DateOfBirth = DateOfBirth,
            Notes       = Notes,
            CreatedAt   = DateTime.Now
        });

        OnSaved?.Invoke();
    }
}
