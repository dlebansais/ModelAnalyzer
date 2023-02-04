namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IObjectRefExprCapsule : IRefExprCapsule
{
    new Expr Item { get; }
    ClassName ClassName { get; }
}
