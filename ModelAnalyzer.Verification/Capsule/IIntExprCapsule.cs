namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IIntExprCapsule : IArithExprCapsule
{
    new IntExpr Item { get; }
}
