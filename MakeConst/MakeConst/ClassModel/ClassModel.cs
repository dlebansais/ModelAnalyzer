namespace DemoAnalyzer;

using System.Collections.Generic;

public record ClassModel
{
    public required string Name { get; init; }
    public Dictionary<FieldName, Field> FieldTable { get; } = new();
    public Dictionary<MethodName, Method> MethodTable { get; } = new();
    public required List<string> InvariantList { get; init; } = new();
    
    public void Verify()
    {
        bool IsInvariantViolated = false;

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            if (Entry.Key.Name == "XYZ")
            {
                IsInvariantViolated = true;
                break;
            }

        ClassModelManager.Instance.SetIsInvariantViolated(Name, IsInvariantViolated);
    }

    public override string ToString()
    {
        string Result = @$"{Name}
";

        foreach (KeyValuePair<FieldName, Field> FieldEntry in FieldTable)
            Result += @$"  int {FieldEntry.Key.Name}
";

        foreach (KeyValuePair<MethodName, Method> MethodEntry in MethodTable)
        {
            Method Method = MethodEntry.Value;
            string Parameters = string.Empty;

            foreach (KeyValuePair<string, Parameter> ParameterEntry in Method.ParameterTable)
            {
                if (Parameters.Length > 0)
                    Parameters += ", ";

                Parameters += ParameterEntry.Key;
            }

            string ReturnString = Method.HasReturnValue ? "int" : "void";
            Result += @$"  {ReturnString} {Method.Name.Name}({Parameters})
";
        }

        return Result;
    }
}
