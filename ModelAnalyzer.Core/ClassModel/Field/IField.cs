namespace DemoAnalyzer;

/// <summary>
/// Provides information about a field.
/// </summary>
public interface IField : IVariable
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    IFieldName FieldName { get; }
}
