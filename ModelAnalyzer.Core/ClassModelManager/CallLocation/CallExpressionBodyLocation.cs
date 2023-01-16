namespace ModelAnalyzer;

/// <summary>
/// Represents the location of a method or function call in an expression body.
/// </summary>
internal class CallExpressionBodyLocation : ICallLocation
{
    /// <inheritdoc/>
    public void RemoveCall()
    {
        // Don't remove anything.
    }
}
