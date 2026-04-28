using OpdManager.Presentation.ViewModels;
using System.Windows;

namespace OpdManager.Presentation.Views;

public partial class AddIssueView : Window
{
    public AddIssueView(AddEditIssueViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}