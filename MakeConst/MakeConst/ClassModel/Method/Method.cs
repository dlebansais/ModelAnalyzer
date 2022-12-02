namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("{MethodName.Name}()")]
public class Method : IMethod
{
    public required MethodName MethodName { get; init; }
    public required bool IsSupported { get; init; }
    public required bool HasReturnValue { get; init; }
    public required ParameterTable ParameterTable { get; init; }
    public required List<IRequire> RequireList { get; init; }
    public required List<IStatement> StatementList { get; init; }
    public required List<IEnsure> EnsureList { get; init; }

    public string Name { get { return MethodName.Name; } }
}
