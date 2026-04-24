using OpdManager.Domain.Entities;
using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpdManager.Presentation.Views;

public partial class AddEditAppointmentView : Window
{
    public AddEditAppointmentView(AddEditAppointmentViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PatientResult_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem { DataContext: Patient patient } &&
            DataContext is AddEditAppointmentViewModel vm)
        {
            vm.SelectPatientCommand.Execute(patient);
        }
    }
}