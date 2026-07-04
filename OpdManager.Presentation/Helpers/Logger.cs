using System.IO;

namespace OpdManager.Presentation.Helpers;

public static class Logger
{
    private static readonly string LogDir  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    private static readonly object _lock   = new();

    public static void Log(string level, string message, Exception? ex = null)
    {
        try
        {
            Directory.CreateDirectory(LogDir);
            var file = Path.Combine(LogDir, $"opd-{DateTime.Today:yyyy-MM-dd}.log");
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            if (ex != null)
                line += $"\n  Exception: {ex.GetType().Name}: {ex.Message}\n  Stack: {ex.StackTrace}";

            lock (_lock)
                File.AppendAllText(file, line + "\n");
        }
        catch { /* never crash on logging */ }
    }

    public static void Info(string message)  => Log("INFO",  message);
    public static void Warn(string message)  => Log("WARN",  message);
    public static void Error(string message, Exception? ex = null) => Log("ERROR", message, ex);
}
