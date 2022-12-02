﻿namespace DemoAnalyzer;

using System.Collections.Generic;

public record Unsupported
{
    public bool InvalidDeclaration { get; set; }
    public bool HasUnsupporteMember { get; set; }
    public List<UnsupportedField> Fields { get; } = new();
    public List<UnsupportedMethod> Methods { get; } = new();
    public List<UnsupportedParameter> Parameters { get; } = new();
    public List<UnsupportedRequire> Requires { get; } = new();
    public List<UnsupportedEnsure> Ensures { get; } = new();
    public List<UnsupportedStatement> Statements { get; } = new();
    public List<UnsupportedExpression> Expressions { get; } = new();
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
