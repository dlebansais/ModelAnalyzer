namespace ModelAnalyzer;

using System;
using Microsoft.Z3;

/// <summary>
/// Represents a binary arithmetic operator.
/// </summary>
internal record BinaryArithmeticOperator(string Text, Func<Context, ArithExpr, ArithExpr, ArithExpr> Asserter);
