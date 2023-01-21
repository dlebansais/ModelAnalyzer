namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

/// <summary>
/// Represents a call to a method statement.
/// </summary>
[DebuggerDisplay("{Name.Text}({string.Join(\", \", ArgumentList)})")]
internal class PrivateMethodCallStatement : Statement, IMethodCallStatement
{
    /// <inheritdoc/>
    required public ClassModel? ClassModel { get; init; }

    /// <inheritdoc/>
    required public MethodName Name { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    required public Location NameLocation { get; init; }

    /// <inheritdoc/>
    required public List<Argument> ArgumentList { get; init; }
}
