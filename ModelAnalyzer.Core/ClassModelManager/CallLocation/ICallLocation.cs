namespace ModelAnalyzer;

/// <summary>
/// Provides information about the location of a method or function call.
/// </summary>
internal interface ICallLocation
{
    /// <summary>
    /// Removes a method or function call at its location.
    /// </summary>
    void RemoveCall();
}
