using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpdManager.Presentation.Views;

public partial class QueueMonitorView : UserControl
{
    public QueueMonitorView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is QueueMonitorViewModel vm)
            await vm.LoadAsync();
    }
}