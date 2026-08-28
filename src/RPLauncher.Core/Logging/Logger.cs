using System.Collections.Concurrent;

namespace RPLauncher.Core.Logging;

public enum LogLevel { Info, Warning, Error }

public record LogEntry(DateTime Timestamp, LogLevel Level, string Message);

public static class Logger
{
    private static readonly object FileLock = new();
    private static readonly ConcurrentQueue<LogEntry> Buffer = new();
    private static string _logFilePath = "";

    public static event Action<LogEntry>? EntryLogged;

    public static void Initialize(string logDirectory)
    {
        try
        {
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"rplauncher-{DateTime.Now:yyyy-MM-dd}.log");
        }
        catch
        {
        }
    }

    public static void Info(string message) => Write(LogLevel.Info, message);
    public static void Warning(string message) => Write(LogLevel.Warning, message);
    public static void Error(string message) => Write(LogLevel.Error, message);

    public static void Error(string message, Exception ex) => Write(LogLevel.Error, $"{message} :: {ex.GetType().Name}: {ex.Message}");

    private static void Write(LogLevel level, string message)
    {
        var entry = new LogEntry(DateTime.Now, level, message);
        Buffer.Enqueue(entry);
        while (Buffer.Count > 2000 && Buffer.TryDequeue(out _)) { }

        try
        {
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                lock (FileLock)
                {
                    File.AppendAllText(_logFilePath, $"[{entry.Timestamp:HH:mm:ss}] [{entry.Level}] {entry.Message}{Environment.NewLine}");
                }
            }
        }
        catch
        {
        }

        try
        {
            EntryLogged?.Invoke(entry);
        }
        catch
        {
        }
    }

    public static IReadOnlyList<LogEntry> GetRecent() => Buffer.ToArray();

    public static string? CurrentLogFile => string.IsNullOrEmpty(_logFilePath) ? null : _logFilePath;
}
