namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents a list of method calls.
/// </summary>
internal record CallSequence
{
    /// <summary>
    /// Gets a value indicating whether the call sequence is empty.
    /// </summary>
    public bool IsEmpty { get => MethodList.Count == 0; }

    /// <summary>
    /// Gets a new call sequence with the provided method as last call.
    /// </summary>
    /// <param name="method">The method to add.</param>
    /// <returns></returns>
    public CallSequence WithAddedCall(Method method)
    {
        CallSequence Result = new();
        Result.MethodList.AddRange(MethodList);
        Result.MethodList.Add(method);

        return Result;
    }

    /// <summary>
    /// Returns an enumerator for the call sequence.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Method> GetEnumerator()
    {
        return MethodList.GetEnumerator();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string Result = string.Empty;

        foreach (Method Method in MethodList)
        {
            if (Result.Length > 0)
                Result += ", ";

            Result += Method.Name;
        }

        return Result;
    }

    private List<Method> MethodList = new();
}
