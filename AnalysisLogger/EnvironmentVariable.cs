namespace AnalysisLogger;

/// <summary>
/// Represents an environment variable.
/// </summary>
public record EnvironmentVariable(string VariableName)
{
    /// <summary>
    /// Converts an environment variable instance to a variable name string.
    /// </summary>
    /// <param name="environmentVariable">The instance to convert.</param>
    public static implicit operator string(EnvironmentVariable environmentVariable) => environmentVariable.VariableName;

    /// <summary>
    /// Converts a variable name string to an environment variable.
    /// </summary>
    /// <param name="variableName">The variable name.</param>
    public static explicit operator EnvironmentVariable(string variableName) => new EnvironmentVariable(variableName);
}
