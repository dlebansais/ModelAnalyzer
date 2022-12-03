namespace DemoAnalyzer;

using System;
using Microsoft.Z3;

internal record LogicalOperator(string Text, Func<Context, BoolExpr, BoolExpr, BoolExpr> Asserter);
