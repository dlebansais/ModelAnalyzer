namespace ModelAnalyzer;

/// <summary>
/// Represents the model of a preloaded method.
/// </summary>
internal partial record PreloadedMethod
{
    /// <summary>
    /// Gets or sets the preloaded method name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preloaded method return type name.
    /// </summary>
    public string ReturnTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    public PreloadedParameter[] Parameters { get; set; } = new PreloadedParameter[0];

    /// <summary>
    /// Gets or sets the require clauses.
    /// </summary>
    public string[] Requires { get; set; } = new string[0];

    /// <summary>
    /// Gets or sets the ensure clauses.
    /// </summary>
    public string[] Ensures { get; set; } = new string[0];
}
