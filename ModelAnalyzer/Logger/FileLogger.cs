namespace DemoAnalyzer;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

public class FileLogger : ILogger
{
#if IN_FILE
    private static readonly Mutex Mutex = new();

    /// <inheritdoc/>
    public void Clear()
    {
        lock (Mutex)
        {
            ClearSync();
        }
    }

    private void ClearSync()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
                Thread.Sleep(10);

            try
            {
                DateTime LastWriteTime = File.GetLastWriteTimeUtc(FilePath);

                if (LastWriteTime + TimeSpan.FromMinutes(1) <= DateTime.UtcNow)
                    WriteFile(FileMode.Create, $"Cleared {DateTime.Now}");
                return;
            }
            catch
            {
            }
        }
    }

    /// <inheritdoc/>
    public void Log(string message)
    {
        lock (Mutex)
        {
            WriteLogSync(message);
        }
    }

    /// <inheritdoc/>
    public void LogException(Exception exception)
    {
        lock (Mutex)
        {
            WriteLogSync(exception.Message);
            WriteLogSync(exception.StackTrace);
        }
    }

    private void WriteLogSync(string message)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
                Thread.Sleep(10);

            try
            {
                WriteFile(FileMode.Append, message);
                return;
            }
            catch
            {
            }
        }
    }

    private const string FilePath = "C:\\Projects\\Temp\\analyzer.txt";

    private void WriteFile(FileMode mode, string message)
    {
        using FileStream Stream = new(FilePath, mode, FileAccess.Write);
        using StreamWriter Writer = new(Stream);

        string FullMessage = $"{Process.GetCurrentProcess().Id,5} {DateTime.Now} {message}";

        Writer.WriteLine(FullMessage);
    }
#else
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return null;
    }
#endif
}
