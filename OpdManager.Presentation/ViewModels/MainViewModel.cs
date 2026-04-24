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

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            DoctorViewModel doctorViewModel,
            AppointmentViewModel appointmentViewModel,
            IServiceProvider serviceProvider)
        {
            _dashboardViewModel   = dashboardViewModel;
            _doctorViewModel      = doctorViewModel;
            _appointmentViewModel = appointmentViewModel;
            _serviceProvider      = serviceProvider;
            _currentView          = _dashboardViewModel;

            _doctorViewModel.OpenDoctorDialog           += OpenDoctorDialog;
            _appointmentViewModel.OpenAppointmentDialog += OpenAppointmentDialog;

            NavDashboardCommand    = new RelayCommand(() => { CurrentView = _dashboardViewModel;   ActiveNav = "Dashboard"; });
            NavDoctorsCommand      = new RelayCommand(() => { CurrentView = _doctorViewModel;       ActiveNav = "Doctors"; });
            NavAppointmentsCommand = new RelayCommand(() => { CurrentView = _appointmentViewModel;  ActiveNav = "Appointments"; });
        }

        private async void OpenAppointmentDialog(AppointmentDto? dto)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditAppointmentViewModel>();
            await vm.InitializeAsync(); // ← await properly

            if (dto != null) vm.LoadAppointment(dto);

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
                _appointmentViewModel.ResetDoctorsCache(); // ← reset so new doctor appears
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