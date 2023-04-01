namespace ModelAnalyzer;

using CodeProverBinding;

internal interface IRefExprCapsule : IExprCapsule
{
    Reference Index { get; }
}
