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
    /// <summary>
    /// Initializes a new instance of the <see cref="FileLogger"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public FileLogger(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLogger"/> class.
    /// </summary>
    /// <param name="environmentVariable">The environment variable.</param>
    /// <param name="fileName">The file name.</param>
    public FileLogger(EnvironmentVariable environmentVariable, string fileName)
    {
        FilePath = Environment.GetEnvironmentVariable(environmentVariable) ?? Path.Combine(Path.GetTempPath(), fileName);
    }

    /// <summary>
    /// Gets the path to the file containing logs.
    /// </summary>
    public string FilePath { get; }

    /// <inheritdoc/>
    public void Log(string message)
    {
        lock (Mutex)
        {
            WriteLogSync(LogLevel.Information, message);
        }
    }

    /// <inheritdoc/>
    public void LogException(Exception exception)
    {
        lock (Mutex)
        {
            WriteLogSync(LogLevel.Error, exception.Message);
            WriteLogSync(LogLevel.Information, exception.StackTrace);
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

    /// <inheritdoc/>
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (Mutex)
        {
            WriteLogSync(logLevel, formatter(state, exception));
        }
    }

    /// <inheritdoc/>
    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return BeginScope(FileMode.Append);
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
                {
                    using StreamWriter Writer = BeginScope(FileMode.Create);

                    string Header = $"Process  Thread                Time (Cleared {DateTime.Now})";
                    Writer.WriteLine(Header);
                }

                return;
            }
            catch
            {
            }
        }
    }

    private void WriteLogSync(LogLevel logLevel, string message)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i > 0)
                Thread.Sleep(10);

            try
            {
                WriteFile(logLevel, message);
                return;
            }
            catch
            {
            }
        }
    }

    private void WriteFile(LogLevel logLevel, string message)
    {
        using StreamWriter Writer = BeginScope(FileMode.Append);

        string LogLevelString = logLevel switch
        {
            LogLevel.Error => "ERROR ",
            LogLevel.Warning => "WARNING ",
            _ => string.Empty,
        };

        string FullMessage = $"{Process.GetCurrentProcess().Id,7} {Thread.CurrentThread.ManagedThreadId,7} {DateTime.Now} {LogLevelString}{message}";

        Writer.WriteLine(FullMessage);
    }

    private StreamWriter BeginScope(FileMode mode)
    {
        FileStream Stream = new(FilePath, mode, FileAccess.Write);
        StreamWriter Writer = new(Stream);

        return Writer;
    }

    private static readonly Mutex Mutex = new();
}
