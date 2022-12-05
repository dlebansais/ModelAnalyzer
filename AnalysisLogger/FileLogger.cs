namespace AnalysisLogger;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a logger for analysis.
/// </summary>
public class FileLogger : IAnalysisLogger
{
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Log(logLevel, formatter(state, exception));
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return BeginScope(FileMode.Append);
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

    /// <inheritdoc/>
    public void Clear()
    {
        lock (Mutex)
        {
            ClearSync();
        }
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    private void Log(LogLevel logLevel, string message)
    {
        lock (Mutex)
        {
            WriteLogSync(message);
        }
    }

    private static readonly Mutex Mutex = new();

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
        using StreamWriter Writer = BeginScope(mode);

        string FullMessage = $"{Process.GetCurrentProcess().Id,5} {Thread.CurrentThread} {DateTime.Now} {message}";

        Writer.WriteLine(FullMessage);
    }

    private StreamWriter BeginScope(FileMode mode)
    {
        FileStream Stream = new(FilePath, mode, FileAccess.Write);
        StreamWriter Writer = new(Stream);

        return Writer;
    }
}
