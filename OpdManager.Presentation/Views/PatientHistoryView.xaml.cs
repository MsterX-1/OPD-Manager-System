using OpdManager.Presentation.ViewModels;
using System.Windows;

namespace OpdManager.Presentation.Views;

public partial class PatientHistoryView : Window
{
    public PatientHistoryView(PatientHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}