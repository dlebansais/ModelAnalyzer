namespace DemoAnalyzer;

public class Parameter : IParameter, IVariable
{
    public required ParameterName ParameterName { get; init; }
    public string Name { get { return ParameterName.Name; } }
}
