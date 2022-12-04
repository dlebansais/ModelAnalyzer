namespace AnalysisLogger;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a logger that just discards messages.
/// </summary>
public class NullLogger : IAnalysisLogger
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
    }

    /// <inheritdoc/>
    public void LogException(Exception exception)
    {
    }
}
