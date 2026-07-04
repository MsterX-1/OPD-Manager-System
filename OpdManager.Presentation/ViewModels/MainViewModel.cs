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
        private readonly QueueMonitorViewModel _queueMonitorViewModel;
        private readonly IssueViewModel _issueViewModel;
        private readonly ReportsViewModel _reportsViewModel;
        private readonly PatientViewModel _patientViewModel;
        private readonly IServiceProvider _serviceProvider;

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
        public RelayCommand NavReportsCommand { get; }
        public RelayCommand NavPatientsCommand { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            DoctorViewModel doctorViewModel,
            AppointmentViewModel appointmentViewModel,
            QueueMonitorViewModel queueMonitorViewModel,
            IssueViewModel issueViewModel,
            ReportsViewModel reportsViewModel,
            PatientViewModel patientViewModel,
            IServiceProvider serviceProvider)
        {
            _dashboardViewModel = dashboardViewModel;
            _doctorViewModel = doctorViewModel;
            _appointmentViewModel = appointmentViewModel;
            _queueMonitorViewModel = queueMonitorViewModel;
            _issueViewModel = issueViewModel;
            _reportsViewModel = reportsViewModel;
            _serviceProvider = serviceProvider;
            _currentView = _dashboardViewModel;
            _patientViewModel = patientViewModel;

            _patientViewModel.OpenPatientHistory += OpenPatientHistory;
            _patientViewModel.OpenCreatePatient   += OpenCreatePatientDialog;

            _doctorViewModel.OpenDoctorDialog += OpenDoctorDialog;
            _appointmentViewModel.OpenAppointmentDialog += OpenAppointmentDialog;
            _issueViewModel.OpenIssueDialog += OpenIssueDialog;

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

            NavReportsCommand = new RelayCommand(() =>
            {
                CurrentView = _reportsViewModel;
                ActiveNav = "Reports";
            });
            NavPatientsCommand = new RelayCommand(async () =>
            {
                CurrentView = _patientViewModel;
                ActiveNav = "Patients";
                await _patientViewModel.LoadAsync();
            });
        }

        private void OpenIssueDialog(Issue? issue)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditIssueViewModel>();

            var dialog = new AddEditIssueView(vm)
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

        private void OpenCreatePatientDialog()
        {
            var vm = _serviceProvider.GetRequiredService<CreatePatientViewModel>();
            var dialog = new CreatePatientView(vm)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            async void OnSaved()
            {
                vm.OnSaved     -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
                await _patientViewModel.LoadAsync();
            }

            void OnCancelled()
            {
                vm.OnSaved     -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
            }

            vm.OnSaved     += OnSaved;
            vm.OnCancelled += OnCancelled;
            dialog.ShowDialog();
        }

        private async void OpenPatientHistory(Domain.Entities.Patient patient)
        {
            var vm = _serviceProvider.GetRequiredService<PatientHistoryViewModel>();
            await vm.LoadAsync(patient);

            var window = new PatientHistoryView(vm)
            {
                Owner = System.Windows.Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog();
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