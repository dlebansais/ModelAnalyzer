namespace DemoAnalyzer;

using System.Collections.Generic;

internal record Unsupported : IUnsupported
{
    public bool InvalidDeclaration { get; set; }
    public bool HasUnsupporteMember { get; set; }

    public IReadOnlyList<IUnsupportedField> Fields => InternalFields.AsReadOnly();

    private List<IUnsupportedField> InternalFields = new();

    public void AddUnsupportedField(IUnsupportedField unsupportedField)
    {
        InternalFields.Add(unsupportedField);
    }

    public IReadOnlyList<IUnsupportedMethod> Methods => InternalMethods.AsReadOnly();

    private List<IUnsupportedMethod> InternalMethods = new();

    public void AddUnsupportedMethod(IUnsupportedMethod unsupportedMethod)
    {
        InternalMethods.Add(unsupportedMethod);
    }

    public IReadOnlyList<IUnsupportedParameter> Parameters => InternalParameters.AsReadOnly();

    private List<IUnsupportedParameter> InternalParameters = new();

    public void AddUnsupportedParameter(IUnsupportedParameter unsupportedParameter)
    {
        InternalParameters.Add(unsupportedParameter);
    }

    public IReadOnlyList<IUnsupportedRequire> Requires => InternalRequires.AsReadOnly();

    private List<IUnsupportedRequire> InternalRequires = new();

    public void AddUnsupportedRequire(IUnsupportedRequire unsupportedRequire)
    {
        InternalRequires.Add(unsupportedRequire);
    }

    public IReadOnlyList<IUnsupportedEnsure> Ensures => InternalEnsures.AsReadOnly();

    private List<IUnsupportedEnsure> InternalEnsures = new();

    public void AddUnsupportedEnsure(IUnsupportedEnsure unsupportedEnsure)
    {
        InternalEnsures.Add(unsupportedEnsure);
    }

    public IReadOnlyList<IUnsupportedStatement> Statements => InternalStatements.AsReadOnly();

    private List<IUnsupportedStatement> InternalStatements = new();

    public void AddUnsupportedStatement(IUnsupportedStatement unsupportedStatement)
    {
        InternalStatements.Add(unsupportedStatement);
    }

    public IReadOnlyList<IUnsupportedExpression> Expressions => InternalExpressions.AsReadOnly();

    private List<IUnsupportedExpression> InternalExpressions = new();

    public void AddUnsupportedExpression(IUnsupportedExpression unsupportedExpression)
    {
        InternalExpressions.Add(unsupportedExpression);
    }

    public IReadOnlyList<IUnsupportedInvariant> Invariants => InternalInvariants.AsReadOnly();

    private List<IUnsupportedInvariant> InternalInvariants = new();

    public void AddUnsupportedInvariant(IUnsupportedInvariant unsupportedInvariant)
    {
        InternalInvariants.Add(unsupportedInvariant);
    }

    public bool IsEmpty => !InvalidDeclaration &&
                           !HasUnsupporteMember &&
                           Fields.Count == 0 &&
                           Methods.Count == 0 &&
                           Parameters.Count == 0 &&
                           Requires.Count == 0 &&
                           Ensures.Count == 0 &&
                           Statements.Count == 0 &&
                           Expressions.Count == 0;
}
