using System.Windows;
using OpdManager.Presentation.Services;
using OpdManager.Presentation.ViewModels;
using OpdManager.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace OpdManager.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        HospitalNameText.Text = AppSettings.Current.HospitalName;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to exit OPD Manager?",
            "Confirm Exit",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            e.Cancel = true;

        base.OnClosing(e);
    }

    private void BackupButton_Click(object sender, RoutedEventArgs e) =>
        BackupService.Backup();

    private void RestoreButton_Click(object sender, RoutedEventArgs e) =>
        BackupService.Restore();

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var vm  = App.Services.GetRequiredService<SettingsViewModel>();
        var dlg = new SettingsView(vm) { Owner = this };

        vm.OnSaved     += () => dlg.Close();
        vm.OnCancelled += () => dlg.Close();

        dlg.ShowDialog();
    }
}
