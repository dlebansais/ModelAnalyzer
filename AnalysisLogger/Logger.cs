namespace AnalysisLogger;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a logger for analysis.
/// </summary>
public class Logger : IAnalysisLogger
{
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

    /// <inheritdoc/>
    public void Log(string message)
    {
        Log(LogLevel.Information, message);
    }

    /// <inheritdoc/>
    public void LogException(Exception exception)
    {
        Log(LogLevel.Error, exception.Message);
        Log(LogLevel.Information, exception.StackTrace);
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="message">The message.</param>
    private void Log(LogLevel logLevel, string message)
    {
    }
}
