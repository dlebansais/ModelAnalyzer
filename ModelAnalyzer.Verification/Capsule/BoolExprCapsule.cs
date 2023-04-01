namespace ModelAnalyzer;

internal record BoolExprCapsule : IBoolExprCapsule, ISimpleExprCapsule
{
    required public CodeProverBinding.IBooleanExpression Item { get; init; }
    CodeProverBinding.IExpression IExprCapsule.Item { get => Item; }
}
