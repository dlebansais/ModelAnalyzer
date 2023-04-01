namespace ModelAnalyzer;

internal interface IObjectRefExprCapsule : IRefExprCapsule
{
    new CodeProverBinding.IExpression Item { get; }
    ClassName ClassName { get; }
}
