namespace ModelAnalyzer;

using Microsoft.Z3;

internal record BoolExprCapsule : IBoolExprCapsule
{
    required public BoolExpr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }
}
