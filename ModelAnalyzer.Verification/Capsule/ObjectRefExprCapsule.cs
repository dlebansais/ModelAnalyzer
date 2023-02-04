namespace ModelAnalyzer;

internal record ObjectRefExprCapsule : RefExprCapsule, IObjectRefExprCapsule
{
    required public ClassName ClassName { get; init; }
}
