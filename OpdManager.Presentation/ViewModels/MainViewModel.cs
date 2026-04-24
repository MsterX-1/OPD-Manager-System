using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly DoctorViewModel _doctorViewModel;

        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public RelayCommand NavDashboardCommand { get; }
        public RelayCommand NavDoctorsCommand { get; }

        public MainViewModel(DashboardViewModel dashboardViewModel,
                             DoctorViewModel doctorViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _doctorViewModel = doctorViewModel;
            _currentView = _dashboardViewModel;

            NavDashboardCommand = new RelayCommand(() => CurrentView = _dashboardViewModel);
            NavDoctorsCommand = new RelayCommand(() => CurrentView = _doctorViewModel);
        }
    }
}
