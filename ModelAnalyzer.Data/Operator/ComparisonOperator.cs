namespace ModelAnalyzer;

using System;
using Microsoft.Z3;

/// <summary>
/// Represents a comparison operator.
/// </summary>
internal record ComparisonOperator(string Text, Func<Context, ArithExpr, ArithExpr, BoolExpr> Asserter);
