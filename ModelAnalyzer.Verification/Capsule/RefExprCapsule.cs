namespace ModelAnalyzer;

internal record RefExprCapsule : IRefExprCapsule
{
    required public CodeProverBinding.IExpression Item { get; init; }
    CodeProverBinding.IExpression IExprCapsule.Item { get => Item; }

    required public CodeProverBinding.Reference Index { get; init; }
}
