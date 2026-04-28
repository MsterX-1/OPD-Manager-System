using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using OpdManager.Presentation.Views;

namespace OpdManager.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly DoctorViewModel _doctorViewModel;
        private readonly AppointmentViewModel _appointmentViewModel;
        private readonly IServiceProvider _serviceProvider;
        private readonly QueueMonitorViewModel _queueMonitorViewModel;
        private readonly IssueViewModel _issueViewModel;

        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private string _activeNav = "Dashboard";
        public string ActiveNav
        {
            get => _activeNav;
            set => SetProperty(ref _activeNav, value);
        }

        public RelayCommand NavDashboardCommand { get; }
        public RelayCommand NavDoctorsCommand { get; }
        public RelayCommand NavAppointmentsCommand { get; }
        public RelayCommand NavQueueMonitorCommand { get; }
        public RelayCommand NavIssuesCommand { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            DoctorViewModel doctorViewModel,
            AppointmentViewModel appointmentViewModel,
            QueueMonitorViewModel queueMonitorViewModel,
            IssueViewModel issueViewModel,
            IServiceProvider serviceProvider)
        {
            _dashboardViewModel = dashboardViewModel;
            _doctorViewModel = doctorViewModel;
            _appointmentViewModel = appointmentViewModel;
            _serviceProvider = serviceProvider;
            _currentView = _dashboardViewModel;
            _queueMonitorViewModel = queueMonitorViewModel;
            _issueViewModel = issueViewModel;

            _doctorViewModel.OpenDoctorDialog += OpenDoctorDialog;
            _appointmentViewModel.OpenAppointmentDialog += OpenAppointmentDialog;
            _issueViewModel.OpenIssueDialog += OpenIssueDialog;

            // Auto-refresh dashboard every time you navigate to it
            NavDashboardCommand = new RelayCommand(async () =>
            {
                CurrentView = _dashboardViewModel;
                ActiveNav = "Dashboard";
                await _dashboardViewModel.LoadAsync();
            });

            NavDoctorsCommand = new RelayCommand(() =>
            {
                CurrentView = _doctorViewModel;
                ActiveNav = "Doctors";
            });

            NavAppointmentsCommand = new RelayCommand(() =>
            {
                CurrentView = _appointmentViewModel;
                ActiveNav = "Appointments";
            });
            NavQueueMonitorCommand = new RelayCommand(async () =>
            {
                CurrentView = _queueMonitorViewModel;
                ActiveNav = "Queue";
                await _queueMonitorViewModel.LoadAsync();
            });

            NavIssuesCommand = new RelayCommand(async () =>
            {
                CurrentView = _issueViewModel;
                ActiveNav = "Issues";
                await _issueViewModel.LoadAsync();
            });


        }
        private void OpenIssueDialog(Issue? issue)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditIssueViewModel>();

            var dialog = new AddIssueView(vm)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            async void OnSaved()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
                await _issueViewModel.LoadAsync();
            }

            void OnCancelled()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
            }

            vm.OnSaved += OnSaved;
            vm.OnCancelled += OnCancelled;
            dialog.ShowDialog();
        }

        private async void OpenAppointmentDialog(AppointmentDto? dto)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditAppointmentViewModel>();

            if (dto != null)
                vm.LoadAppointment(dto);

            await vm.InitializeAsync();

            var dialog = new AddEditAppointmentView(vm)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            async void OnSaved()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
                await _appointmentViewModel.LoadAsync();
            }

            void OnCancelled()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
            }

            vm.OnSaved += OnSaved;
            vm.OnCancelled += OnCancelled;

            dialog.ShowDialog();
        }

        private void OpenDoctorDialog(Doctor? doctor)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditDoctorViewModel>();
            if (doctor != null) vm.LoadDoctor(doctor);

            var dialog = new AddEditDoctorView(vm)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            async void OnSaved()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
                await _doctorViewModel.LoadAsync();
                _appointmentViewModel.ResetDoctorsCache();
            }

            void OnCancelled()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
            }

            vm.OnSaved += OnSaved;
            vm.OnCancelled += OnCancelled;
            dialog.ShowDialog();
        }
    }
}