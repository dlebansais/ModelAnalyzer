namespace ModelAnalyzer;

using Microsoft.Z3;

internal record ArithExprCapsule : IArithExprCapsule, ISimpleExprCapsule
{
    required public ArithExpr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }
}
