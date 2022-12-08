namespace ModelAnalyzer;

using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Represents a compilation context.
/// </summary>
public record CompilationContext
{
    /// <summary>
    /// Gets the default compilation context.
    /// </summary>
    public static CompilationContext Default { get; } = new CompilationContext();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> class.
    /// </summary>
    private CompilationContext()
    {
        HashCode = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> class.
    /// </summary>
    private CompilationContext(int newHashCode)
    {
        HashCode = newHashCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> class.
    /// </summary>
    /// <param name="context">The syntax node analysis context.</param>
    public CompilationContext(SyntaxNodeAnalysisContext context)
    {
        HashCode = context.Compilation.GetHashCode();
    }

    /// <summary>
    /// Gets a new compilation context to force verification.
    /// </summary>
    public static CompilationContext GetAnother()
    {
        int NewHashCode = Interlocked.Increment(ref UsedHashCodes);
        return new CompilationContext(NewHashCode);
    }

    private int HashCode;
    private static int UsedHashCodes;
}
