using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Enums;
using OpdManager.Presentation.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class QueueMonitorViewModel : BaseViewModel
{
    private readonly IQueueService _queueService;

    private ObservableCollection<QueueEntryDto> _queue = new();
    private QueueEntryDto? _selectedEntry;
    private bool _isLoading;

    public ObservableCollection<QueueEntryDto> Queue
    {
        get => _queue;
        set => SetProperty(ref _queue, value);
    }

    public QueueEntryDto? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            SetProperty(ref _selectedEntry, value);
            CallNextCommand.RaiseCanExecuteChanged();
            MarkDoneCommand.RaiseCanExecuteChanged();
            MarkNoShowCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // Summary stats
    private int _waitingCount;
    private int _inProgressCount;
    private int _doneCount;

    public int WaitingCount
    {
        get => _waitingCount;
        set => SetProperty(ref _waitingCount, value);
    }
    public int InProgressCount
    {
        get => _inProgressCount;
        set => SetProperty(ref _inProgressCount, value);
    }
    public int DoneCount
    {
        get => _doneCount;
        set => SetProperty(ref _doneCount, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand CallNextCommand { get; }
    public RelayCommand MarkDoneCommand { get; }
    public RelayCommand MarkNoShowCommand { get; }

    public QueueMonitorViewModel(IQueueService queueService)
    {
        _queueService = queueService;

        RefreshCommand = new RelayCommand(async () => await LoadAsync());

        CallNextCommand = new RelayCommand(
            async () => await UpdateStatusAsync(QueueStatus.InProgress),
            () => SelectedEntry != null &&
                  SelectedEntry.Status == QueueStatus.Waiting);

        MarkDoneCommand = new RelayCommand(
            async () => await UpdateStatusAsync(QueueStatus.Done),
            () => SelectedEntry != null &&
                  SelectedEntry.Status == QueueStatus.InProgress);

        MarkNoShowCommand = new RelayCommand(
            async () => await UpdateStatusAsync(QueueStatus.Skipped),
            () => SelectedEntry != null &&
                  SelectedEntry.Status == QueueStatus.Waiting);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var queue = await _queueService.GetTodayQueueAsync();
            Queue = new ObservableCollection<QueueEntryDto>(queue);

            // Update summary counts
            WaitingCount = Queue.Count(q => q.Status == QueueStatus.Waiting);
            InProgressCount = Queue.Count(q => q.Status == QueueStatus.InProgress);
            DoneCount = Queue.Count(q => q.Status == QueueStatus.Done);
        }
        finally { IsLoading = false; }
    }

    private async Task UpdateStatusAsync(QueueStatus status)
    {
        if (SelectedEntry is null) return;

        // Capture before reload clears selection
        var entryId = SelectedEntry.Id;
        var patientName = SelectedEntry.PatientName;

        try
        {
            await _queueService.UpdateStatusAsync(entryId, status);
            await LoadAsync();

            var message = status switch
            {
                QueueStatus.InProgress => $"Calling {patientName}...",
                QueueStatus.Done => $"{patientName} consultation done.",
                QueueStatus.Skipped => $"{patientName} marked as No Show.",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(message))
                MessageBox.Show(message, "Queue Update",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update status: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}