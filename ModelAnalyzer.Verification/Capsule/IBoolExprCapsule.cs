namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IBoolExprCapsule : IExprCapsule
{
    new BoolExpr Item { get; }
}
