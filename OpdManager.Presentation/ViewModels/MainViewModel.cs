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

        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Nav commands
        public RelayCommand NavDashboardCommand { get; }

        public MainViewModel(DashboardViewModel dashboardViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _currentView = _dashboardViewModel;

            NavDashboardCommand = new RelayCommand(() =>
                CurrentView = _dashboardViewModel);
        }
    }
}
