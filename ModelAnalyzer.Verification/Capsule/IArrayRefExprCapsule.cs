namespace ModelAnalyzer;

internal interface IArrayRefExprCapsule : IRefExprCapsule
{
    new CodeProverBinding.IExpression Item { get; }
    ExpressionType ElementType { get; }
}
