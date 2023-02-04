namespace ModelAnalyzer;

internal record ArrayRefExprCapsule : RefExprCapsule, IArrayRefExprCapsule
{
    required public ExpressionType ElementType { get; init; }
}
