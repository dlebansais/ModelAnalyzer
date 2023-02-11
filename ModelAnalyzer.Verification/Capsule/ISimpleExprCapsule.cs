namespace ModelAnalyzer;

internal interface ISimpleExprCapsule
{
    IExprSingle<IExprCapsule> ToSingle();
}
