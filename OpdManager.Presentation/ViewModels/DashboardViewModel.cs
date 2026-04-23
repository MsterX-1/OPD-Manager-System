using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.Interfaces;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        // Stats
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

        public DashboardViewModel(IUnitOfWork uow)
        {
            _uow = uow;
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var today = DateTime.Today;
                var todayAppointments = (await _uow.Appointments.GetByDateAsync(today)).ToList();
                var todayQueue = (await _uow.Queue.GetTodayQueueAsync()).ToList();
                var doctors = (await _uow.Doctors.GetActiveAsync()).ToList();
                var issues = (await _uow.Issues.GetByStatusAsync(Domain.Enums.IssueStatus.Open)).ToList();

                TotalPatientsToday = todayAppointments.Count;

                WaitingPatients = todayQueue
                    .Count(q => q.Status == Domain.Enums.QueueStatus.Waiting);

                InProgressPatients = todayQueue
                    .Count(q => q.Status == Domain.Enums.QueueStatus.InProgress);

                CompletedPatients = todayQueue
                    .Count(q => q.Status == Domain.Enums.QueueStatus.Done);

                var completedWithTimes = todayQueue
                    .Where(q => q.Status == Domain.Enums.QueueStatus.Done && q.StartTime.HasValue)
                    .ToList();

                AverageWaitingMinutes = completedWithTimes.Any()
                    ? completedWithTimes.Average(q => (q.StartTime!.Value - q.CheckInTime).TotalMinutes)
                    : 0;

                ActiveDoctors = doctors.Count;
                OpenIssues = issues.Count;
                LastUpdated = $"Last updated: {DateTime.Now:hh:mm tt}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
