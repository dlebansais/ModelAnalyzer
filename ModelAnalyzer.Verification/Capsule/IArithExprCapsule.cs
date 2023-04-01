namespace ModelAnalyzer;

internal interface IArithExprCapsule : IExprCapsule
{
    new CodeProverBinding.IArithmeticExpression Item { get; }
}
