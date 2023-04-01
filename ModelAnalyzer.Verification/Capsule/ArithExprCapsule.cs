namespace ModelAnalyzer;

internal record ArithExprCapsule : IArithExprCapsule, ISimpleExprCapsule
{
    required public CodeProverBinding.IArithmeticExpression Item { get; init; }
    CodeProverBinding.IExpression IExprCapsule.Item { get => Item; }
}
