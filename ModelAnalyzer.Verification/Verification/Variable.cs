namespace ModelAnalyzer;

/// <summary>
/// Represents a variable.
/// </summary>
internal record Variable(IVariableName Name, ExpressionType Type) : IVariable;
