using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;

namespace OpdManager.Presentation.ViewModels;

public class AddEditIssueViewModel : BaseViewModel
{
    private readonly IIssueService _issueService;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _category = "Delay";
    private string _dialogTitle = "New Issue";

    public string Title
    {
        get => _title;
        set { SetProperty(ref _title, value); SaveCommand.RaiseCanExecuteChanged(); }
    }

    public string Description
    {
        get => _description;
        set { SetProperty(ref _description, value); SaveCommand.RaiseCanExecuteChanged(); }
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string DialogTitle
    {
        get => _dialogTitle;
        set => SetProperty(ref _dialogTitle, value);
    }

    public ObservableCollection<string> Categories { get; } = new()
    {
        "Delay", "Complaint", "Equipment", "Staff", "Other"
    };

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event Action? OnSaved;
    public event Action? OnCancelled;

    public AddEditIssueViewModel(IIssueService issueService)
    {
        _issueService = issueService;

        SaveCommand = new RelayCommand(
            async () => await SaveAsync(),
            () => !string.IsNullOrWhiteSpace(Title) &&
                  !string.IsNullOrWhiteSpace(Description));

        CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
    }

    private async Task SaveAsync()
    {
        await _issueService.AddAsync(new Issue
        {
            Title = Title,
            Description = Description,
            Category = Category,
            ReportedAt = DateTime.Now
        });

        OnSaved?.Invoke();
    }
}