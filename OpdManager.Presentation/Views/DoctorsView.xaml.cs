using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.ViewModels;

namespace OpdManager.Presentation.Views
{
    public partial class DoctorsView : UserControl
    {
        public DoctorsView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DoctorViewModel vm)
                await vm.LoadAsync();
        }
    }
}
