using OpdManager.Application.Services.Interfaces;
using OpdManager.Presentation.Core;
using OpdManager.Presentation.Services;
using System.Windows.Threading;

namespace OpdManager.Presentation.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IQueueService _queueService;
        private readonly DispatcherTimer _refreshTimer;

        private int _totalPatientsToday;
        private int _waitingPatients;
        private int _inProgressPatients;
        private int _completedPatients;
        private double _averageWaitingMinutes;
        private int _activeDoctors;
        private int _openIssues;
        private bool _isLoading;
        private string _lastUpdated = string.Empty;
        private bool _hasWaitingAlert;
        private string _alertMessage = string.Empty;

        // Alert threshold — patients waiting more than this many minutes
        private const int AlertThresholdMinutes = 30;

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
        public bool HasWaitingAlert
        {
            get => _hasWaitingAlert;
            set => SetProperty(ref _hasWaitingAlert, value);
        }
        public string AlertMessage
        {
            get => _alertMessage;
            set => SetProperty(ref _alertMessage, value);
        }

        public RelayCommand RefreshCommand { get; }

        public DashboardViewModel(IQueueService queueService)
        {
            _queueService  = queueService;
            RefreshCommand = new RelayCommand(async () => await LoadAsync());

            // Auto-refresh every 60 seconds
            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _refreshTimer.Tick += async (s, e) => await LoadAsync();
            _refreshTimer.Start();
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var stats = await _queueService.GetDashboardStatsAsync();

                TotalPatientsToday    = stats.TotalPatientsToday;
                WaitingPatients       = stats.WaitingPatients;
                InProgressPatients    = stats.InProgressPatients;
                CompletedPatients     = stats.CompletedPatients;
                AverageWaitingMinutes = stats.AverageWaitingMinutes;
                ActiveDoctors         = stats.ActiveDoctors;
                OpenIssues            = stats.OpenIssues;
                LastUpdated           = $"Last updated: {DateTime.Now:h:mm:ss tt}";

                // Alert: average wait exceeds threshold from settings
                var threshold = AppSettings.Current.WaitingAlertMinutes;
                if (stats.AverageWaitingMinutes >= threshold && stats.WaitingPatients > 0)
                {
                    HasWaitingAlert = true;
                    AlertMessage    = $"⚠ {stats.WaitingPatients} patient(s) waiting over {threshold} min — avg {stats.AverageWaitingMinutes:F0} min";
                }
                else
                {
                    HasWaitingAlert = false;
                    AlertMessage    = string.Empty;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void StopTimer() => _refreshTimer.Stop();
        public void StartTimer() => _refreshTimer.Start();
    }
}