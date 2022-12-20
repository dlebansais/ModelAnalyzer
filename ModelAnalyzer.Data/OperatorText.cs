namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorText
{
    /// <summary>
    /// Gets the text associated to a binary arithmetic operator.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, string> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, "+" },
        { BinaryArithmeticOperator.Subtract, "-" },
        { BinaryArithmeticOperator.Multiply, "*" },
        { BinaryArithmeticOperator.Divide, "/" },
    };

    /// <summary>
    /// Gets the text associated to a unary arithmetic operator.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, string> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, "-" },
    };

    /// <summary>
    /// Gets the text associated to a binary logical operator.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, string> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, "||" },
        { BinaryLogicalOperator.And, "&&" },
    };

    /// <summary>
    /// Gets the text associated to a unary logical operator.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, string> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, "!" },
    };

    /// <summary>
    /// Gets the text associated to an equality operator.
    /// </summary>
    public static Dictionary<EqualityOperator, string> Equality { get; } = new()
    {
        { EqualityOperator.Equal, "==" },
        { EqualityOperator.NotEqual, "!=" },
    };

    /// <summary>
    /// Gets the text associated to a comparison operator.
    /// </summary>
    public static Dictionary<ComparisonOperator, string> Comparison { get; } = new()
    {
        { ComparisonOperator.GreaterThan, ">" },
        { ComparisonOperator.GreaterThanOrEqual, ">=" },
        { ComparisonOperator.LessThan, "<" },
        { ComparisonOperator.LessThanOrEqual, "<=" },
    };
}
