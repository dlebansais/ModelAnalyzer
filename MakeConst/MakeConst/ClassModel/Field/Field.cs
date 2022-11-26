namespace DemoAnalyzer;

public class Field : IField, IVariable
{
    public required FieldName FieldName { get; init; }
    public string Name { get { return FieldName.Name; } }
}
