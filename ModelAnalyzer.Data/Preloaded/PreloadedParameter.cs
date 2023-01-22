namespace ModelAnalyzer;

/// <summary>
/// Represents the model of a preloaded method parameter.
/// </summary>
internal partial record PreloadedParameter
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter type name.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
}
