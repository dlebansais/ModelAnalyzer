namespace ModelAnalyzer;

/// <summary>
/// Type of an expression.
/// </summary>
public enum ExpressionType
{
    /// <summary>
    /// Unknown type.
    /// </summary>
    Other,

    /// <summary>
    /// No type.
    /// </summary>
    Void,

    /// <summary>
    /// bool type.
    /// </summary>
    Boolean,

    /// <summary>
    /// int type.
    /// </summary>
    Integer,

    /// <summary>
    /// double type.
    /// </summary>
    FloatingPoint,
}
