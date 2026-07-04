using OpdManager.Presentation.ViewModels;
using System.Windows;

namespace OpdManager.Presentation.Views;

public partial class CreatePatientView : Window
{
    public CreatePatientView(CreatePatientViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
