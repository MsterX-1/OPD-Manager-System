using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class IssueViewModel : BaseViewModel
{
    private readonly IIssueService _issueService;

    private ObservableCollection<Issue> _issues = new();
    private Issue? _selectedIssue;
    private bool _isLoading;
    private bool _showOpenOnly = true;

    public ObservableCollection<Issue> Issues
    {
        get => _issues;
        set => SetProperty(ref _issues, value);
    }

    public Issue? SelectedIssue
    {
        get => _selectedIssue;
        set
        {
            SetProperty(ref _selectedIssue, value);
            CloseCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool ShowOpenOnly
    {
        get => _showOpenOnly;
        set
        {
            SetProperty(ref _showOpenOnly, value);
            _ = LoadAsync();
        }
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand CloseCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public event Action<Issue?>? OpenIssueDialog;

    public IssueViewModel(IIssueService issueService)
    {
        _issueService = issueService;

        AddCommand = new RelayCommand(() => OpenIssueDialog?.Invoke(null));
        RefreshCommand = new RelayCommand(async () => await LoadAsync());

        CloseCommand = new RelayCommand(
            async () => await CloseIssueAsync(),
            () => SelectedIssue != null && SelectedIssue.Status == IssueStatus.Open);

        DeleteCommand = new RelayCommand(
            async () => await DeleteIssueAsync(),
            () => SelectedIssue != null);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var issues = ShowOpenOnly
                ? await _issueService.GetOpenAsync()
                : await _issueService.GetAllAsync();
            Issues = new ObservableCollection<Issue>(issues);
        }
        finally { IsLoading = false; }
    }

    private async Task CloseIssueAsync()
    {
        if (SelectedIssue is null) return;

        // Capture before reload clears selection
        var issueId = SelectedIssue.Id;

        await _issueService.CloseAsync(issueId, "Resolved");
        await LoadAsync();
    }

    private async Task DeleteIssueAsync()
    {
        if (SelectedIssue is null) return;

        var result = MessageBox.Show(
            $"Delete issue: {SelectedIssue.Title}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        // Capture before reload clears selection
        var issueId = SelectedIssue.Id;

        await _issueService.DeleteAsync(issueId);
        await LoadAsync();
    }
}