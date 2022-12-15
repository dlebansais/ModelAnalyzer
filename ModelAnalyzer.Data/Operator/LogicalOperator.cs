namespace ModelAnalyzer;

using System;
using Microsoft.Z3;

/// <summary>
/// Represents a logical operator.
/// </summary>
internal record LogicalOperator(string Text, Func<Context, BoolExpr, BoolExpr, BoolExpr> Asserter);
