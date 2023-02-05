namespace ModelAnalyzer;

using Microsoft.Z3;

internal record ArrayExprCapsule : IArrayExprCapsule
{
    required public ArrayExpr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }
}
