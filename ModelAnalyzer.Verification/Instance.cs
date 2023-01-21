namespace ModelAnalyzer;

/// <summary>
/// Represents an instance of a class.
/// </summary>
internal record Instance
{
    /// <summary>
    /// Gets the class model.
    /// </summary>
    required public ClassModel ClassModel { get; init; }

    /// <summary>
    /// Gets instance reference expression.
    /// </summary>
    required public IRefExprCapsule Expr { get; init; }
}
