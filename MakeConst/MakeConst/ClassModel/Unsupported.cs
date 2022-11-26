namespace DemoAnalyzer;

using System.Collections.Generic;

public record Unsupported
{
    public bool HasUnsupporteMember { get; set; }
    public List<UnsupportedField> Fields { get; } = new();
    public List<UnsupportedMethod> Methods { get; } = new();
    public List<UnsupportedParameter> Parameters { get; } = new();
    public List<UnsupportedStatement> Statements { get; } = new();
    public bool IsEmpty => !HasUnsupporteMember &&
                           Fields.Count == 0 &&
                           Methods.Count == 0 &&
                           Parameters.Count == 0 &&
                           Statements.Count == 0;
}
