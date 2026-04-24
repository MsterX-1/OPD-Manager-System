using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using OpdManager.Presentation.ViewModels;

namespace OpdManager.Presentation.Views
{
    public partial class AddEditDoctorView : Window
    {
        public AddEditDoctorView(AddEditDoctorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
