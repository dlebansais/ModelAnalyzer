namespace ModelAnalyzer;

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
    public CompilationContext()
    {
        HashCode = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationContext"/> class.
    /// </summary>
    /// <param name="context">The syntax node analysis context.</param>
    public CompilationContext(SyntaxNodeAnalysisContext context)
    {
        HashCode = context.Compilation.GetHashCode();
    }

    private int HashCode;
}
