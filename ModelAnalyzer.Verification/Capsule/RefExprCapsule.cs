namespace ModelAnalyzer;

using Microsoft.Z3;

internal record RefExprCapsule : IRefExprCapsule
{
    required public Expr Item { get; init; }
    Expr IExprCapsule.Item { get => Item; }

    required public string ClassName { get; init; }
    required public int Index { get; init; }
}
