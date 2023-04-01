namespace ModelAnalyzer;

internal record IntExprCapsule : IIntExprCapsule, ISimpleExprCapsule
{
    required public CodeProverBinding.IIntegerExpression Item { get; init; }
    CodeProverBinding.IArithmeticExpression IArithExprCapsule.Item { get => Item; }
    CodeProverBinding.IExpression IExprCapsule.Item { get => Item; }
}
