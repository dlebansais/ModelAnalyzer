namespace ModelAnalyzer;

using Microsoft.Z3;

internal record ExprCapsule : IExprCapsule
{
    required public Expr Item { get; init; }
}
