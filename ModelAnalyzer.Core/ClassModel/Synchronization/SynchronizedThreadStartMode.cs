namespace ModelAnalyzer;

/// <summary>
/// Represents the start modes of a synchronized thread.
/// </summary>
public enum SynchronizedThreadStartMode
{
    /// <summary>
    /// Automatic start, as soon as there is a class to verify.
    /// </summary>
    Auto,

    /// <summary>
    /// Manual start.
    /// </summary>
    Manual,
}
