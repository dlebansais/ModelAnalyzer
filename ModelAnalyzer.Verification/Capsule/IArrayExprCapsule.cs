namespace ModelAnalyzer;

internal interface IArrayExprCapsule : IExprCapsule
{
    new CodeProverBinding.IExpression Item { get; }

    ExpressionType ElementType { get; }
}
