using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpdManager.Presentation.Views;

public partial class AppointmentsView : UserControl
{
    public AppointmentsView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AppointmentViewModel vm)
            await vm.LoadAsync();
    }
}