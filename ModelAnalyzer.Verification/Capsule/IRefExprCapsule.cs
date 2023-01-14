namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IRefExprCapsule : IExprCapsule
{
    new Expr Item { get; }
}
