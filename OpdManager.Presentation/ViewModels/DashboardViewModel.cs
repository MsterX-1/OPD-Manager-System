using OpdManager.Application.Services.Interfaces;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IQueueService _queueService; 

        private int _totalPatientsToday;
        private int _waitingPatients;
        private int _inProgressPatients;
        private int _completedPatients;
        private double _averageWaitingMinutes;
        private int _activeDoctors;
        private int _openIssues;
        private bool _isLoading;
        private string _lastUpdated = string.Empty;

        public int TotalPatientsToday
        {
            get => _totalPatientsToday;
            set => SetProperty(ref _totalPatientsToday, value);
        }
        public int WaitingPatients
        {
            get => _waitingPatients;
            set => SetProperty(ref _waitingPatients, value);
        }
        public int InProgressPatients
        {
            get => _inProgressPatients;
            set => SetProperty(ref _inProgressPatients, value);
        }
        public int CompletedPatients
        {
            get => _completedPatients;
            set => SetProperty(ref _completedPatients, value);
        }
        public double AverageWaitingMinutes
        {
            get => _averageWaitingMinutes;
            set => SetProperty(ref _averageWaitingMinutes, value);
        }
        public int ActiveDoctors
        {
            get => _activeDoctors;
            set => SetProperty(ref _activeDoctors, value);
        }
        public int OpenIssues
        {
            get => _openIssues;
            set => SetProperty(ref _openIssues, value);
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

        public RelayCommand RefreshCommand { get; }

        public DashboardViewModel(IQueueService queueService)
        {
            _queueService = queueService;
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                // Use service which has correct business logic
                var stats = await _queueService.GetDashboardStatsAsync();

                TotalPatientsToday = stats.TotalPatientsToday;
                WaitingPatients = stats.WaitingPatients;
                InProgressPatients = stats.InProgressPatients;
                CompletedPatients = stats.CompletedPatients;
                AverageWaitingMinutes = stats.AverageWaitingMinutes;
                ActiveDoctors = stats.ActiveDoctors;
                OpenIssues = stats.OpenIssues;
                LastUpdated = $"Last updated: {DateTime.Now:hh:mm tt}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}