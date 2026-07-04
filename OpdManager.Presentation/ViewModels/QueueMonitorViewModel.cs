using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Enums;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace OpdManager.Presentation.ViewModels;

public class QueueMonitorViewModel : BaseViewModel
{
    private readonly IQueueService _queueService;
    private readonly DispatcherTimer _refreshTimer;

    private ObservableCollection<QueueEntryDto> _waitingQueue = new();
    private ObservableCollection<QueueEntryDto> _inProgressQueue = new();
    private ObservableCollection<QueueEntryDto> _completedQueue = new();
    private QueueEntryDto? _selectedEntry;
    private bool _isLoading;
    private string _lastUpdated = string.Empty;

    public ObservableCollection<QueueEntryDto> WaitingQueue
    {
        get => _waitingQueue;
        set => SetProperty(ref _waitingQueue, value);
    }

    public ObservableCollection<QueueEntryDto> InProgressQueue
    {
        get => _inProgressQueue;
        set => SetProperty(ref _inProgressQueue, value);
    }

    public ObservableCollection<QueueEntryDto> CompletedQueue
    {
        get => _completedQueue;
        set => SetProperty(ref _completedQueue, value);
    }

    public QueueEntryDto? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            SetProperty(ref _selectedEntry, value);
            CallNextCommand.RaiseCanExecuteChanged();
            MarkDoneCommand.RaiseCanExecuteChanged();
            SkipCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    // Summary stats
    public int TotalWaiting => WaitingQueue.Count;
    public int TotalInProgress => InProgressQueue.Count;
    public int TotalCompleted => CompletedQueue.Count;

    public RelayCommand RefreshCommand { get; }
    public RelayCommand CallNextCommand { get; }
    public RelayCommand MarkDoneCommand { get; }
    public RelayCommand SkipCommand { get; }

    public QueueMonitorViewModel(IQueueService queueService)
    {
        _queueService = queueService;

        RefreshCommand = new RelayCommand(async () => await LoadAsync());

        CallNextCommand = new RelayCommand(
            async () => await UpdateStatusAsync(QueueStatus.InProgress),
            () => SelectedEntry?.Status == QueueStatus.Waiting);

        MarkDoneCommand = new RelayCommand(
            async () => await UpdateStatusAsync(QueueStatus.Done),
            () => SelectedEntry?.Status == QueueStatus.InProgress);

        SkipCommand = new RelayCommand(
            async () => await SkipAsync(),
            () => SelectedEntry?.Status == QueueStatus.Waiting);

        // Auto-refresh every 30 seconds
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += async (s, e) => await LoadAsync();
        _refreshTimer.Start();
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var queue = (await _queueService.GetTodayQueueAsync()).ToList();

            WaitingQueue = new ObservableCollection<QueueEntryDto>(
                queue.Where(q => q.Status == QueueStatus.Waiting));
            InProgressQueue = new ObservableCollection<QueueEntryDto>(
                queue.Where(q => q.Status == QueueStatus.InProgress));
            CompletedQueue = new ObservableCollection<QueueEntryDto>(
                queue.Where(q => q.Status == QueueStatus.Done));

            // Notify summary stats changed
            OnPropertyChanged(nameof(TotalWaiting));
            OnPropertyChanged(nameof(TotalInProgress));
            OnPropertyChanged(nameof(TotalCompleted));

            LastUpdated = $"Last updated: {DateTime.Now:h:mm:ss tt}";
        }
        finally { IsLoading = false; }
    }

    private async Task UpdateStatusAsync(QueueStatus status)
    {
        if (SelectedEntry is null) return;

        var id = SelectedEntry.Id;
        var patientName = SelectedEntry.PatientName;

        await _queueService.UpdateStatusAsync(id, status);
        await LoadAsync();

        var msg = status == QueueStatus.InProgress
            ? $"Calling {patientName} — Queue #{SelectedEntry?.QueueNumber}"
            : $"{patientName} consultation marked as done.";

        MessageBox.Show(msg, "Queue Update",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SkipAsync()
    {
        if (SelectedEntry is null) return;

        var result = MessageBox.Show(
            $"Skip {SelectedEntry.PatientName}? This will mark them as No Show.",
            "Confirm Skip",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var id = SelectedEntry.Id;
        await _queueService.UpdateStatusAsync(id, QueueStatus.Skipped);
        await LoadAsync();
    }

    // Stop timer when navigating away
    public void StopTimer() => _refreshTimer.Stop();
    public void StartTimer() => _refreshTimer.Start();
}