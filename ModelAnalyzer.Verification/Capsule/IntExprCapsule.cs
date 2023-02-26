namespace ModelAnalyzer;

using Microsoft.Z3;

internal record IntExprCapsule : IIntExprCapsule, ISimpleExprCapsule
{
    required public IntExpr Item { get; init; }
    ArithExpr IArithExprCapsule.Item { get => Item; }
    Expr IExprCapsule.Item { get => Item; }
}
