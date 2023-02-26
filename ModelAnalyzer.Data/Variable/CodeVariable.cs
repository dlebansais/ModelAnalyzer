﻿namespace ModelAnalyzer;

using System.Diagnostics;

/// <summary>
/// Represents a variable.
/// </summary>
[DebuggerDisplay("{Name.Text}")]
internal record CodeVariable : IVariable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeVariable"/> class.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="type">The field type.</param>
    public CodeVariable(IVariableName name, ExpressionType type)
    {
        Name = name;
        Type = type;
    }

    /// <inheritdoc/>
    public IVariableName Name { get; }

    /// <inheritdoc/>
    public ExpressionType Type { get; }
}
