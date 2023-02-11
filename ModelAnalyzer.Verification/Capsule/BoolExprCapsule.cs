namespace ModelAnalyzer;

using Microsoft.Z3;

internal record BoolExprCapsule : IBoolExprCapsule, ISimpleExprCapsule
{
    required public BoolExpr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }

    public IExprSingle<IExprCapsule> ToSingle() => new ExprSingle<IBoolExprCapsule>(this);
}
