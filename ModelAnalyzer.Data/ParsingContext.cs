namespace ModelAnalyzer;

using System.Collections.Generic;

/// <summary>
/// Represents the context to use when parsing a class.
/// </summary>
internal record ParsingContext
{
    public Unsupported Unsupported { get; } = new();
    public FieldTable FieldTable { get; set; } = new();
    public bool IsMethodParsingStarted { get; set; }
    public MethodTable MethodTable { get; set; } = new();
    public bool IsMethodParsingComplete { get; set; }
    public Method? HostMethod { get; set; }
    public List<Invariant> InvariantList { get; set; } = new();
    public bool IsLocalAllowed { get; set; }
    public Local? ResultLocal { get; set; }
}
