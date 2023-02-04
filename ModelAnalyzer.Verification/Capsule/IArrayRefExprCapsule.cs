namespace ModelAnalyzer;

using Microsoft.Z3;

internal interface IArrayRefExprCapsule : IRefExprCapsule
{
    new Expr Item { get; }
    ExpressionType ElementType { get; }
}
