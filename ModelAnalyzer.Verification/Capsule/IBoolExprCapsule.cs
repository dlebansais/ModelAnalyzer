namespace ModelAnalyzer;

internal interface IBoolExprCapsule : IExprCapsule
{
    new CodeProverBinding.IBooleanExpression Item { get; }
}
