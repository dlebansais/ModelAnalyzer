namespace DemoAnalyzer;

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{Field.FieldName.Name} {OperatorText} {ConstantValue}")]
public class Invariant : IInvariant
{
    public required string Text { get; init; }
    public required IField Field { get; init; }
    public required SyntaxKind OperatorKind { get; init; }
    public required int ConstantValue { get; init; }

    public string FieldName { get { return Field.FieldName.Name; } }
}
