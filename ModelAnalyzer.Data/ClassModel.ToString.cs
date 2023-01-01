﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
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

    /// <summary>
    /// Gets a variable from its name.
    /// </summary>
    /// <param name="parsingContext">The parsing context.</param>
    /// <param name="resultLocal">The optional result local.</param>
    /// <param name="variableName">The variable name.</param>
    public static IVariable GetVariable(ParsingContext parsingContext, Local? resultLocal, IVariableName variableName)
    {
        IVariable? Result = null;

        foreach (KeyValuePair<FieldName, Field> Entry in parsingContext.FieldTable)
            if (Entry.Key.Text == variableName.Text)
            {
                Result = Entry.Value;
                break;
            }

        if (parsingContext.HostMethod is not null)
        {
            foreach (KeyValuePair<ParameterName, Parameter> Entry in parsingContext.HostMethod.ParameterTable)
                if (Entry.Key.Text == variableName.Text)
                {
                    Debug.Assert(Result is null);

                    Result = Entry.Value;
                    break;
                }

            foreach (KeyValuePair<LocalName, Local> Entry in parsingContext.HostMethod.LocalTable)
                if (Entry.Key.Text == variableName.Text)
                {
                    Debug.Assert(Result is null);

                    Result = Entry.Value;
                    break;
                }
        }

        if (Result is null && resultLocal is not null && resultLocal.Name.Text == variableName.Text)
            Result = resultLocal;

        Debug.Assert(Result is not null);

        return Result!;
    }

    private void AppendClassName(StringBuilder builder)
    {
        builder.AppendLine(Name);
    }

    private void AppendFields(StringBuilder builder)
    {
        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            AppendField(builder, Entry.Value);
    }

    private void AppendField(StringBuilder builder, Field field)
    {
        string TypeString = ExpressionTypeToString(field.Type);

        string InitializerString;
        ILiteralExpression? Initializer = field.Initializer;

        if (Initializer is not null)
            InitializerString = $" = {Initializer}";
        else
            InitializerString = string.Empty;

        builder.AppendLine($"  {TypeString} {field.Name.Text}{InitializerString}");
    }

    private void AppendMethods(StringBuilder builder)
    {
        bool IsFirst = true;

        foreach (KeyValuePair<MethodName, Method> MethodEntry in MethodTable)
        {
            if (!IsFirst || !FieldTable.IsEmpty)
                builder.AppendLine();

            MethodToString(builder, MethodEntry.Value);
            IsFirst = false;
        }
    }

    private void MethodToString(StringBuilder builder, Method method)
    {
        string Modifier = method.AccessModifier == AccessModifier.Public ? "public " : string.Empty;

        string Parameters = string.Empty;

        foreach (KeyValuePair<ParameterName, Parameter> ParameterEntry in method.ParameterTable)
        {
            if (Parameters.Length > 0)
                Parameters += ", ";

            Parameter Parameter = ParameterEntry.Value;
            string ParameterTypeString = ExpressionTypeToString(Parameter.Type);
            Parameters += $"{ParameterTypeString} {Parameter.Name.Text}";
        }

        string ReturnTypeString = ExpressionTypeToString(method.ReturnType);
        builder.AppendLine($"  {Modifier}{ReturnTypeString} {method.Name.Text}({Parameters})");

        foreach (Require Require in method.RequireList)
            AppendAssertion(builder, "require", Require.BooleanExpression);

        AppendIndentation(builder, indentation: 0);
        builder.AppendLine("{");

        AppendLocals(builder, method.LocalTable);
        AppendStatements(builder, method.StatementList, 0);

        AppendIndentation(builder, indentation: 0);
        builder.AppendLine("}");

        foreach (Ensure Ensure in method.EnsureList)
            AppendAssertion(builder, "ensure", Ensure.BooleanExpression);
    }

    private void AppendAssertion(StringBuilder builder, string text, IExpression booleanExpression)
    {
        builder.AppendLine($"  # {text} {booleanExpression}");
    }

    private void AppendLocals(StringBuilder builder, ReadOnlyLocalTable localTable)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in localTable)
            AppendLocal(builder, Entry.Value);
    }

    private void AppendLocal(StringBuilder builder, Local local)
    {
        string TypeString = ExpressionTypeToString(local.Type);

        string InitializerString;
        ILiteralExpression? Initializer = local.Initializer;

        if (Initializer is not null)
            InitializerString = $" = {Initializer}";
        else
            InitializerString = string.Empty;

        builder.AppendLine($"    {TypeString} {local.Name.Text}{InitializerString}");
    }

    private void AppendBlock(StringBuilder builder, List<Statement> statementList, int indentation)
    {
        if (statementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("{");
        }

        AppendStatements(builder, statementList, indentation);

        if (statementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("}");
        }
    }

    private void AppendStatements(StringBuilder builder, List<Statement> statementList, int indentation)
    {
        foreach (Statement Item in statementList)
            AppendStatement(builder, Item, indentation + 1);
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
            case MethodCallStatement MethodCall:
                AppendMethodCallStatement(builder, MethodCall, indentation);
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
        AppendStatementText(builder, $"{statement.DestinationName.Text} = {statement.Expression}", indentation);
    }

    private void AppendConditionalStatement(StringBuilder builder, ConditionalStatement statement, int indentation)
    {
        AppendIndentation(builder, indentation);
        builder.AppendLine($"if ({statement.Condition})");
        AppendBlock(builder, statement.WhenTrueStatementList, indentation);

        if (statement.WhenFalseStatementList.Count > 0)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("else");
            AppendBlock(builder, statement.WhenFalseStatementList, indentation);
        }
    }

    private void AppendMethodCallStatement(StringBuilder builder, MethodCallStatement statement, int indentation)
    {
        AppendStatementText(builder, $"{statement.MethodName.Text}({string.Join(", ", statement.ArgumentList)})", indentation);
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