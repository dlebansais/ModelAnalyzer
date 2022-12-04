namespace AnalysisLogger;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides a logger for model analysis.
/// </summary>
public interface IAnalysisLogger : ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message.</param>
    void Log(string message);

    /// <summary>
    /// Logs an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    void LogException(Exception exception);
}