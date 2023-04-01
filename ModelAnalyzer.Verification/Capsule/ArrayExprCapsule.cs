namespace ModelAnalyzer;

internal record ArrayExprCapsule : IArrayExprCapsule
{
    required public CodeProverBinding.IExpression Item { get; init; }
    CodeProverBinding.IExpression IExprCapsule.Item { get => Item; }

    required public ExpressionType ElementType { get; init; }
}
