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
            {
                vm.OpenDoctorDialog += OpenDoctorDialog;
                await vm.LoadAsync();
            }
        }

        private void OpenDoctorDialog(Doctor? doctor)
        {
            var vm = App.Services.GetRequiredService<AddEditDoctorViewModel>();

            if (doctor != null)
                vm.LoadDoctor(doctor);

            var dialog = new AddEditDoctorView(vm) { Owner = Window.GetWindow(this) };

            vm.OnSaved += async () =>
            {
                dialog.Close();
                if (DataContext is DoctorViewModel listVm)
                    await listVm.LoadAsync();
            };

            vm.OnCancelled += () => dialog.Close();
            dialog.ShowDialog();
        }
    }
}
