namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Represents a return statement.
/// </summary>
[DebuggerDisplay("{MethodName.Text}({string.Join(\", \", ArgumentList)})")]
internal class MethodCallStatement : Statement
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public MethodName MethodName { get; init; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    required public List<IExpression> ArgumentList { get; init; }
}
