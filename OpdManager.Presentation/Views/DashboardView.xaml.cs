using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpdManager.Presentation.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            vm.StartTimer();
            await vm.LoadAsync();
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
            vm.StopTimer();
    }
}
