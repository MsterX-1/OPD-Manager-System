using OpdManager.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpdManager.Presentation.Views;

public partial class IssuesView : UserControl
{
    public IssuesView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is IssueViewModel vm)
            await vm.LoadAsync();
    }
}