namespace DemoAnalyzer;

using System;

/// <summary>
/// Provides a mechanism for logging messages.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Clears the log.
    /// </summary>
    void Clear();

    /// <summary>
    /// Adds a message to the log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    void Log(string message);

    /// <summary>
    /// Adds a message from an exception to the log.
    /// </summary>
    /// <param name="exception">The exception.</param>
    void LogException(Exception exception);
}
