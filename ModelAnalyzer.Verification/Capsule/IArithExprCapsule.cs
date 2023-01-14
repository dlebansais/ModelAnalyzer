namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IArithExprCapsule : IExprCapsule
{
    new ArithExpr Item { get; }
}
