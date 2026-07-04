using System.IO;
using System.Text.Json;

namespace OpdManager.Presentation.Services;

public class AppSettings
{
    public string HospitalName        { get; set; } = "OPD Manager";
    public int    WaitingAlertMinutes { get; set; } = 30;
    public string BackupFolder        { get; set; } = "";

    // ── persistence ──────────────────────────────────────────────────────────
    private static readonly string Path =
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    private static AppSettings? _instance;
    public  static AppSettings  Current => _instance ??= Load();

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { /* fall through to defaults */ }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path, json);
        }
        catch { }
    }
}
