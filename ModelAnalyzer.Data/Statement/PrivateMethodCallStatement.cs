namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a return statement.
/// </summary>
[DebuggerDisplay("{MethodName.Text}({string.Join(\", \", ArgumentList)})")]
internal class PrivateMethodCallStatement : Statement
{
    /// <summary>
    /// Gets the method name.
    /// </summary>
    required public MethodName MethodName { get; init; }

    /// <summary>
    /// Gets the method name location.
    /// </summary>
    [JsonIgnore]
    required public Location NameLocation { get; init; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    required public List<Argument> ArgumentList { get; init; }
}
