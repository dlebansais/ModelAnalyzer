namespace DemoAnalyzer;

using System.Collections.Generic;

public class Method : IMethod
{
    public required MethodName Name { get; init; }
    public required bool HasReturnValue { get; init; }
    public required Dictionary<string, Parameter> ParameterTable { get; init; }
}
