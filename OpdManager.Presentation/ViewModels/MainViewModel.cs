using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;
using OpdManager.Presentation.Views;

namespace OpdManager.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly DoctorViewModel _doctorViewModel;
        private readonly IServiceProvider _serviceProvider;

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

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            DoctorViewModel doctorViewModel,
            IServiceProvider serviceProvider)
        {
            _dashboardViewModel = dashboardViewModel;
            _doctorViewModel = doctorViewModel;
            _serviceProvider = serviceProvider;
            _currentView = _dashboardViewModel;

            // Subscribe ONCE here — MainViewModel lives for the whole app lifetime
            _doctorViewModel.OpenDoctorDialog += OpenDoctorDialog;

            NavDashboardCommand = new RelayCommand(() =>
            {
                CurrentView = _dashboardViewModel;
                ActiveNav = "Dashboard";
            });

            NavDoctorsCommand = new RelayCommand(() =>
            {
                CurrentView = _doctorViewModel;
                ActiveNav = "Doctors";
            });
        }

        private void OpenDoctorDialog(Doctor? doctor)
        {
            var vm = _serviceProvider.GetRequiredService<AddEditDoctorViewModel>();

            if (doctor != null)
                vm.LoadDoctor(doctor);

            var parentWindow = System.Windows.Application.Current.MainWindow;
            var dialog = new AddEditDoctorView(vm)
            {
                Owner = parentWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            async void OnSaved()
            {
                vm.OnSaved -= OnSaved;
                vm.OnCancelled -= OnCancelled;
                dialog.Close();
                await _doctorViewModel.LoadAsync();
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
