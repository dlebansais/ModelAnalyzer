namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IArrayExprCapsule : IExprCapsule
{
    new ArrayExpr Item { get; }
}
