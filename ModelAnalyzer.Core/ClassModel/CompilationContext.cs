namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Represents a compilation context.
/// </summary>
public struct CompilationContext
{
    /// <summary>
    /// The first value for incrementally growing hash codes.
    /// </summary>
    private const long FirstCollisionlessHashCode = 0x7FFFFFFF00000000;

    /// <summary>
    /// Gets the default compilation context.
    /// </summary>
    public static CompilationContext Default { get; } = new CompilationContext(FirstCollisionlessHashCode);

    /// <summary>
    /// Gets a new compilation context to force verification.
    /// </summary>
    public static CompilationContext GetAnother()
    {
        long NewHashCode = Interlocked.Increment(ref UsedHashCodes);
        return new CompilationContext(NewHashCode);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> struct.
    /// </summary>
    /// <param name="newHashCode">The hash code initialization the context.</param>
    private CompilationContext(long newHashCode)
    {
        HashCode = newHashCode;
        IsAsyncRunRequested = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> struct.
    /// </summary>
    /// <param name="diagnosticId">A diagnostic id.</param>
    /// <param name="obj">An object whose hash code is used to initialize the context.</param>
    /// <param name="isAsyncRunRequested">Whether this context is to request an asynchronous run.</param>
    public CompilationContext(string diagnosticId, object obj, bool isAsyncRunRequested)
    {
        long newHashCode = obj.GetHashCode();

        if (HashCode == newHashCode)
        {
            // If the hash code is the same for a diagnostic id already encountered, this hash code is tainted.
            // We handle this by increasing the counter so that comparison with a context with the same hash code will report not compatible.
            if (DiagnosticIdList.Contains(diagnosticId))
            {
                DiagnosticIdList.Clear();
                SameHashCounter++;
            }

            DiagnosticIdList.Add(diagnosticId);
        }
        else
        {
            HashCode = newHashCode;
            SameHashCounter = 0;
            DiagnosticIdList.Clear();
        }

        IsAsyncRunRequested = isAsyncRunRequested;
    }

    /// <summary>
    /// Gets a unique value indentifying the compilation context.
    /// </summary>
    internal long HashCode { get; }

    /// <summary>
    /// Gets the number of time the same hash code has been reused.
    /// </summary>
    internal int SameHashCounter { get; }

    /// <summary>
    /// Gets the list of diagnostic ids that have been used for a particular pair of hash code and counter.
    /// </summary>
    internal List<string> DiagnosticIdList { get; } = new();

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
        if (HashCode != other.HashCode)
            return false;

        if (SameHashCounter != other.SameHashCounter)
            return false;

        if (!IsAsyncRunRequested && other.IsAsyncRunRequested)
            return false;

        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{HashCode:X16};{SameHashCounter:X2};{IsAsyncRunRequested}";
    }

    // By starting at high value, we exploit the fact that object.GetHashCode() returns an int and therefore cannot collide with this global value.
    private static long UsedHashCodes = FirstCollisionlessHashCode;
}
