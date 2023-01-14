namespace ModelAnalyzer;

using Microsoft.Z3;

internal record ArithExprCapsule : IArithExprCapsule
{
    required public ArithExpr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }
}
