namespace ModelAnalyzer;

internal interface IRefExprCapsule : IExprCapsule
{
    ReferenceIndex Index { get; }
}
