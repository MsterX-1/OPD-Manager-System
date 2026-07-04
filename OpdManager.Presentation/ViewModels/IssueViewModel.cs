using OpdManager.Application.DTOs;
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
    private const int PageSize = 20;

    private ObservableCollection<Issue> _issues = new();
    private Issue? _selectedIssue;
    private bool _isLoading;
    private string _searchText = string.Empty;
    private string? _filterCategory;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private string _statusText = string.Empty;
    private StatusFilterOption _selectedStatusOption;

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
            ReviewCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string? FilterCategory
    {
        get => _filterCategory;
        set => SetProperty(ref _filterCategory, value);
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

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public StatusFilterOption SelectedStatusOption
    {
        get => _selectedStatusOption;
        set => SetProperty(ref _selectedStatusOption, value);
    }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext     => CurrentPage < TotalPages;

    public ObservableCollection<string> Categories { get; } = new()
        { "All", "Delay", "Complaint", "Equipment", "Staff", "Other" };

    public ObservableCollection<StatusFilterOption> StatusOptions { get; } = new()
    {
        new("All",       null),
        new("Open",      IssueStatus.Open),
        new("In Review", IssueStatus.InReview),
        new("Closed",    IssueStatus.Closed)
    };

    public RelayCommand AddCommand          { get; }
    public RelayCommand ReviewCommand       { get; }
    public RelayCommand CloseCommand        { get; }
    public RelayCommand DeleteCommand       { get; }
    public RelayCommand SearchCommand       { get; }
    public RelayCommand ClearSearchCommand  { get; }
    public RelayCommand PreviousPageCommand { get; }
    public RelayCommand NextPageCommand     { get; }

    public event Action<Issue?>? OpenIssueDialog;

    public IssueViewModel(IIssueService issueService)
    {
        _issueService = issueService;
        _selectedStatusOption = new StatusFilterOption("All", null);

        AddCommand = new RelayCommand(() => OpenIssueDialog?.Invoke(null));

        CloseCommand = new RelayCommand(async () => await CloseIssueAsync(),
            () => SelectedIssue?.Status == IssueStatus.Open ||
                  SelectedIssue?.Status == IssueStatus.InReview);

        ReviewCommand = new RelayCommand(async () => await MarkInReviewAsync(),
            () => SelectedIssue?.Status == IssueStatus.Open);

        DeleteCommand = new RelayCommand(async () => await DeleteIssueAsync(),
            () => SelectedIssue != null);

        SearchCommand = new RelayCommand(async () => await LoadAsync(1));

        ClearSearchCommand = new RelayCommand(async () =>
        {
            SearchText           = string.Empty;
            FilterCategory       = null;
            SelectedStatusOption = StatusOptions[0];
            await LoadAsync(1);
        });

        PreviousPageCommand = new RelayCommand(async () =>
        {
            if (CanGoPrevious) await LoadAsync(CurrentPage - 1);
        });

        NextPageCommand = new RelayCommand(async () =>
        {
            if (CanGoNext) await LoadAsync(CurrentPage + 1);
        });
    }

    public async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        try
        {
            var category = FilterCategory == "All" || FilterCategory == null
                ? null : FilterCategory;
            var status = SelectedStatusOption?.Value;

            var paged = await _issueService.SearchPagedAsync(
                string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                category, status, page, PageSize);

            Issues      = new ObservableCollection<Issue>(paged.Items);
            CurrentPage = page;
            TotalPages  = paged.TotalPages == 0 ? 1 : paged.TotalPages;
            TotalCount  = paged.TotalCount;
            StatusText  = $"{TotalCount} issues found";

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
        finally { IsLoading = false; }
    }

    private async Task MarkInReviewAsync()
    {
        if (SelectedIssue is null) return;
        var id = SelectedIssue.Id;
        await _issueService.MarkInReviewAsync(id);
        await LoadAsync(CurrentPage);
    }

    private async Task CloseIssueAsync()
    {
        if (SelectedIssue is null) return;

        var resolution = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter resolution details:", "Close Issue", "Resolved");

        if (string.IsNullOrWhiteSpace(resolution)) return;

        await _issueService.CloseAsync(SelectedIssue.Id, resolution);
        await LoadAsync(CurrentPage);
    }

    private async Task DeleteIssueAsync()
    {
        if (SelectedIssue is null) return;
        var id    = SelectedIssue.Id;
        var title = SelectedIssue.Title;

        var result = MessageBox.Show(
            $"Delete issue: \"{title}\"?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        await _issueService.DeleteAsync(id);
        await LoadAsync(CurrentPage);
    }
}

public record StatusFilterOption(string Label, IssueStatus? Value);