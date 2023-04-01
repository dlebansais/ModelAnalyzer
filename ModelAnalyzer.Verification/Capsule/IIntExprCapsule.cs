namespace ModelAnalyzer;

internal interface IIntExprCapsule : IArithExprCapsule
{
    new CodeProverBinding.IIntegerExpression Item { get; }
}
