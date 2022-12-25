namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
    /// <summary>
    /// Gets the class name.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public FieldTable FieldTable { get; init; }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public MethodTable MethodTable { get; init; }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public List<Invariant> InvariantList { get; init; }

    /// <summary>
    /// Gets unsupported class elements.
    /// </summary>
    required public Unsupported Unsupported { get; init; }

    /// <inheritdoc/>
    required public bool IsInvariantViolated { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder Builder = new();

        AppendClassName(Builder);
        AppendFields(Builder);
        AppendMethods(Builder);
        AppendInvariants(Builder);

        return TextBuilder.Normalized(Builder.ToString());
    }

    private void AppendClassName(StringBuilder builder)
    {
        builder.AppendLine(Name);
    }

    private void AppendFields(StringBuilder builder)
    {
        foreach (KeyValuePair<FieldName, Field> FieldEntry in FieldTable)
        {
            Field Field = FieldEntry.Value;
            string TypeString = ExpressionTypeToString(Field.VariableType);

            string InitializerString;
            ILiteralExpression? Initializer = Field.Initializer;

            if (Initializer is not null)
                InitializerString = $" = {Initializer}";
            else
                InitializerString = string.Empty;

            builder.AppendLine($"  {TypeString} {Field.Name}{InitializerString}");
        }
    }

    private void AppendMethods(StringBuilder builder)
    {
        foreach (KeyValuePair<MethodName, Method> MethodEntry in MethodTable)
            MethodToString(builder, MethodEntry.Value);
    }

    private void MethodToString(StringBuilder builder, Method method)
    {
        string Parameters = string.Empty;

        foreach (KeyValuePair<ParameterName, Parameter> ParameterEntry in method.ParameterTable)
        {
            if (Parameters.Length > 0)
                Parameters += ", ";

            Parameter Parameter = ParameterEntry.Value;
            string ParameterTypeString = ExpressionTypeToString(Parameter.VariableType);
            Parameters += $"{ParameterTypeString} {Parameter.Name}";
        }

        string ReturnTypeString = ExpressionTypeToString(method.ReturnType);
        builder.AppendLine($"  {ReturnTypeString} {method.Name}({Parameters})");

        foreach (Require Require in method.RequireList)
            AppendAssertion(builder, "require", Require.BooleanExpression);

        AppendStatements(builder, method.StatementList, 0, forceBraces: true);

        foreach (Ensure Ensure in method.EnsureList)
            AppendAssertion(builder, "ensure", Ensure.BooleanExpression);
    }

    private void AppendAssertion(StringBuilder builder, string text, Expression booleanExpression)
    {
        builder.AppendLine($"  # {text} {booleanExpression}");
    }

    private void AppendStatements(StringBuilder builder, List<Statement> statementList, int indentation, bool forceBraces = false)
    {
        if (forceBraces || statementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("{");
        }

        foreach (Statement Item in statementList)
            AppendStatement(builder, Item, indentation + 1);

        if (forceBraces || statementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("}");
        }
    }

    private void AppendStatement(StringBuilder builder, Statement statement, int indentation)
    {
        bool IsHandled = false;

        switch (statement)
        {
            case AssignmentStatement Assignment:
                AppendAssignmentStatement(builder, Assignment, indentation);
                IsHandled = true;
                break;
            case ConditionalStatement Conditional:
                AppendConditionalStatement(builder, Conditional, indentation);
                IsHandled = true;
                break;
            case ReturnStatement Return:
                AppendReturnStatement(builder, Return, indentation);
                IsHandled = true;
                break;
        }

        Debug.Assert(IsHandled);
    }

    private void AppendAssignmentStatement(StringBuilder builder, AssignmentStatement statement, int indentation)
    {
        AppendStatementText(builder, $"{statement.Destination.Name} = {statement.Expression}", indentation);
    }

    private void AppendConditionalStatement(StringBuilder builder, ConditionalStatement statement, int indentation)
    {
        AppendIndentation(builder, indentation);
        builder.AppendLine($"if ({statement.Condition})");
        AppendStatements(builder, statement.WhenTrueStatementList, indentation);

        if (statement.WhenFalseStatementList.Count > 0)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("else");
            AppendStatements(builder, statement.WhenFalseStatementList, indentation);
        }
    }

    private void AppendReturnStatement(StringBuilder builder, ReturnStatement statement, int indentation)
    {
        if (statement.Expression is null)
            AppendStatementText(builder, "return", indentation);
        else
            AppendStatementText(builder, $"return {statement.Expression}", indentation);
    }

    private void AppendStatementText(StringBuilder builder, string text, int indentation)
    {
        AppendIndentation(builder, indentation);
        builder.AppendLine($"{text};");
    }

    private void AppendIndentation(StringBuilder builder, int indentation)
    {
        for (int i = 0; i < indentation + 1; i++)
            builder.Append("  ");
    }

    private void AppendInvariants(StringBuilder builder)
    {
        foreach (Invariant Invariant in InvariantList)
            builder.AppendLine($"  * {Invariant.BooleanExpression}");
    }

    private string ExpressionTypeToString(ExpressionType expressionType)
    {
        Dictionary<ExpressionType, string> ExpressionTypeToStringTable = new()
        {
            { ExpressionType.Void, "void" },
            { ExpressionType.Boolean, "bool" },
            { ExpressionType.Integer, "int" },
            { ExpressionType.FloatingPoint, "double" },
        };

        Debug.Assert(ExpressionTypeToStringTable.ContainsKey(expressionType));

        return ExpressionTypeToStringTable[expressionType];
    }
}
