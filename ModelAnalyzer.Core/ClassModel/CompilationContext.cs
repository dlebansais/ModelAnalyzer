namespace ModelAnalyzer;

using System.Threading;

/// <summary>
/// Represents a compilation context.
/// </summary>
public struct CompilationContext
{
    /// <summary>
    /// The first value for incrementally growing hash codes.
    /// </summary>
    private const long UsedHashCodesFirst = 0x7FFFFFFF00000000;

    /// <summary>
    /// Gets the default compilation context.
    /// </summary>
    public static CompilationContext Default { get; } = new CompilationContext(UsedHashCodesFirst, isAsyncRunRequested: false);

    /// <summary>
    /// Gets a new compilation context to force verification.
    /// </summary>
    public static CompilationContext GetAnother()
    {
        long NewHashCode = Interlocked.Increment(ref UsedHashCodes);
        return new CompilationContext(NewHashCode, isAsyncRunRequested: false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> struct.
    /// </summary>
    /// <param name="newHashCode">The hash code initialization the context.</param>
    /// <param name="isAsyncRunRequested">Whether an asynchronous run is already request.</param>
    internal CompilationContext(long newHashCode, bool isAsyncRunRequested)
    {
        HashCode = newHashCode;
        IsAsyncRunRequested = isAsyncRunRequested;
    }

    /// <summary>
    /// Gets a unique value indentifying the compilation context.
    /// </summary>
    internal long HashCode { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the comilation context has at least one asynchrounous run requested.
    /// </summary>
    internal bool IsAsyncRunRequested { get; set; }

    /// <summary>
    /// Checks whether the current instance is compatible with another.
    /// </summary>
    /// <param name="other">The other instance.</param>
    public bool IsCompatibleWith(CompilationContext other)
    {
        return HashCode == other.HashCode && (IsAsyncRunRequested || IsAsyncRunRequested == other.IsAsyncRunRequested);
    }

    // By starting at high value, we exploit the fact that object.GetHashCode() returns an int and therefore cannot collide with this global value.
    private static long UsedHashCodes = UsedHashCodesFirst;
}
