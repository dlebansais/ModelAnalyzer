namespace DemoAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("{MethodName.Name}()")]
public class Method : IMethod
{
    required public MethodName MethodName { get; init; }
    required public bool IsSupported { get; init; }
    required public bool HasReturnValue { get; init; }
    required public ParameterTable ParameterTable { get; init; }
    required public List<IRequire> RequireList { get; init; }
    required public List<IStatement> StatementList { get; init; }
    required public List<IEnsure> EnsureList { get; init; }

    public string Name { get { return MethodName.Name; } }
}
