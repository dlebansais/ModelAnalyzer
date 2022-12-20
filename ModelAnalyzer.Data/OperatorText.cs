namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Provides tables of supported operators.
/// </summary>
internal static class OperatorText
{
    /// <summary>
    /// Gets supported binary arithmetic operators.
    /// </summary>
    public static Dictionary<BinaryArithmeticOperator, string> BinaryArithmetic { get; } = new()
    {
        { BinaryArithmeticOperator.Add, "+" },
        { BinaryArithmeticOperator.Subtract, "-" },
        { BinaryArithmeticOperator.Multiply, "*" },
        { BinaryArithmeticOperator.Divide, "/" },
    };

    /// <summary>
    /// Gets supported unary arithmetic operators.
    /// </summary>
    public static Dictionary<UnaryArithmeticOperator, string> UnaryArithmetic { get; } = new()
    {
        { UnaryArithmeticOperator.Minus, "-" },
    };

    /// <summary>
    /// Gets supported binary logical operators.
    /// </summary>
    public static Dictionary<BinaryLogicalOperator, string> BinaryLogical { get; } = new()
    {
        { BinaryLogicalOperator.Or, "||" },
        { BinaryLogicalOperator.And, "&&" },
    };

    /// <summary>
    /// Gets supported unary logical operators.
    /// </summary>
    public static Dictionary<UnaryLogicalOperator, string> UnaryLogical { get; } = new()
    {
        { UnaryLogicalOperator.Not, "!" },
    };

    /// <summary>
    /// Gets supported comparison operators.
    /// </summary>
    public static Dictionary<ComparisonOperator, string> Comparison { get; } = new()
    {
        { ComparisonOperator.Equal, "==" },
        { ComparisonOperator.NotEqual, "!=" },
        { ComparisonOperator.GreaterThan, ">" },
        { ComparisonOperator.GreaterThanOrEqual, ">=" },
        { ComparisonOperator.LessThan, "<" },
        { ComparisonOperator.LessThanOrEqual, "<=" },
    };
}
