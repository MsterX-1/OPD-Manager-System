using OpdManager.Application.DTOs;
using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        {
            vm.StartTimer();
            await vm.LoadAsync();
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is QueueMonitorViewModel vm)
            vm.StopTimer();
    }

    private void QueueCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border { DataContext: QueueEntryDto entry } &&
            DataContext is QueueMonitorViewModel vm)
        {
            vm.SelectedEntry = entry;
        }
    }
}