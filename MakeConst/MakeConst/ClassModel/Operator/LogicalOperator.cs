namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

public record LogicalOperator(string Text, Func<Context, BoolExpr, BoolExpr, BoolExpr> Asserter);
