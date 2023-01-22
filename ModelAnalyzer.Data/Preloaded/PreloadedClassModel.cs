namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the model of a preloaded class.
/// </summary>
internal partial record PreloadedClassModel
{
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the methods.
    /// </summary>
    public PreloadedMethod[] Methods { get; set; } = new PreloadedMethod[0];
}
