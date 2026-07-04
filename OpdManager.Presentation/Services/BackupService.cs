using System.IO;
using Microsoft.Win32;
using OpdManager.Presentation.Helpers;
using System.Windows;

namespace OpdManager.Presentation.Services;

public static class BackupService
{
    private static readonly string DbPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opd.db");

    public static void Backup()
    {
        try
        {
            if (!File.Exists(DbPath))
            {
                MessageBox.Show("Database file not found.", "Backup",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title            = "Save Backup",
                Filter           = "SQLite Database|*.db",
                FileName         = $"opd_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog() != true) return;

            File.Copy(DbPath, dlg.FileName, overwrite: true);
            Logger.Info($"Backup created: {dlg.FileName}");

            MessageBox.Show($"Backup saved successfully to:\n{dlg.FileName}",
                "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error("Backup failed", ex);
            MessageBox.Show($"Backup failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static void Restore()
    {
        try
        {
            var dlg = new OpenFileDialog
            {
                Title            = "Select Backup File",
                Filter           = "SQLite Database|*.db",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog() != true) return;

            var confirm = MessageBox.Show(
                "Restoring will replace the current database.\n\nAre you sure? The app will restart.",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            File.Copy(dlg.FileName, DbPath, overwrite: true);
            Logger.Info($"Database restored from: {dlg.FileName}");

            MessageBox.Show("Database restored. The application will now restart.",
                "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);

            // Restart app
            System.Windows.Application.Current.Shutdown();
            System.Diagnostics.Process.Start(
                System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName!);
        }
        catch (Exception ex)
        {
            Logger.Error("Restore failed", ex);
            MessageBox.Show($"Restore failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
