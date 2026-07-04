using OpdManager.Presentation.ViewModels;
using System.Windows;

namespace OpdManager.Presentation.Views;

public partial class AddEditIssueView : Window
{
    public AddEditIssueView(AddEditIssueViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}