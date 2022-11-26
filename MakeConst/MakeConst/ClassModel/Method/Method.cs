namespace DemoAnalyzer;

using System.Collections.Generic;

public class Method : IMethod
{
    public required MethodName MethodName { get; init; }
    public required bool HasReturnValue { get; init; }
    public required Dictionary<ParameterName, IParameter> ParameterTable { get; init; }
    public required List<IStatement> StatementList { get; init; }
}
