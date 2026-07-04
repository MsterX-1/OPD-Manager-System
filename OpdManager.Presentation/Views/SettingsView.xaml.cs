using OpdManager.Presentation.ViewModels;
using System.Windows;

namespace OpdManager.Presentation.Views;

public partial class SettingsView : Window
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
