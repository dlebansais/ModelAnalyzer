namespace ModelAnalyzer;

/// <summary>
/// Provides information about a literal expression.
/// </summary>
/// <typeparam name="T">The literal value type.</typeparam>
public interface ILiteralExpression<T>
{
    /// <summary>
    /// Gets or sets the literal value.
    /// </summary>
    public T Value { get; set; }
}
