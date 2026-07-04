using OpdManager.Presentation.Core;
using OpdManager.Presentation.Services;
using System.Windows;

namespace OpdManager.Presentation.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _hospitalName;
    private int    _waitingAlertMinutes;
    private string _backupFolder;

    public string HospitalName
    {
        get => _hospitalName;
        set => SetProperty(ref _hospitalName, value);
    }

    public int WaitingAlertMinutes
    {
        get => _waitingAlertMinutes;
        set => SetProperty(ref _waitingAlertMinutes, value);
    }

    public string BackupFolder
    {
        get => _backupFolder;
        set => SetProperty(ref _backupFolder, value);
    }

    public RelayCommand SaveCommand   { get; }
    public RelayCommand CancelCommand { get; }

    public event Action? OnSaved;
    public event Action? OnCancelled;

    public SettingsViewModel()
    {
        var s = AppSettings.Current;
        _hospitalName        = s.HospitalName;
        _waitingAlertMinutes = s.WaitingAlertMinutes;
        _backupFolder        = s.BackupFolder;

        SaveCommand   = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());
    }

    private void Save()
    {
        if (WaitingAlertMinutes < 1 || WaitingAlertMinutes > 120)
        {
            MessageBox.Show("Alert threshold must be between 1 and 120 minutes.",
                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var s = AppSettings.Current;
        s.HospitalName        = HospitalName;
        s.WaitingAlertMinutes = WaitingAlertMinutes;
        s.BackupFolder        = BackupFolder;
        s.Save();

        OnSaved?.Invoke();
    }
}
