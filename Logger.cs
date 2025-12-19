using System.Text;

namespace ClipShare;

public static class Logger
{
    private static readonly object _lock = new();
    private static string? _logFilePath;
    private static bool _enabled = true;

    public static void Initialize()
    {
        try
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClipShare"
            );
            Directory.CreateDirectory(appDataPath);
            
            string logFileName = $"clipshare_{DateTime.Now:yyyyMMdd}.log";
            _logFilePath = Path.Combine(appDataPath, logFileName);
            
            // Clean up old log files (keep only last 7 days)
            CleanupOldLogs(appDataPath);
        }
        catch
        {
            _enabled = false;
        }
    }

    private static void CleanupOldLogs(string logDirectory)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-7);
            var logFiles = Directory.GetFiles(logDirectory, "clipshare_*.log");
            
            foreach (var logFile in logFiles)
            {
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(logFile);
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public static void Log(string message)
    {
        WriteLog("INFO", message);
    }

    public static void LogError(string message)
    {
        WriteLog("ERROR", message);
    }

    public static void LogWarning(string message)
    {
        WriteLog("WARN", message);
    }

    private static void WriteLog(string level, string message)
    {
        if (!_enabled || string.IsNullOrEmpty(_logFilePath))
            return;

        try
        {
            lock (_lock)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry, Encoding.UTF8);
            }
        }
        catch
        {
            // Silently fail if logging fails
        }
    }

    public static string GetLogFilePath()
    {
        return _logFilePath ?? "Log file not initialized";
    }
}
