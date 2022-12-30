namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/// <summary>
/// Represents the model of a class.
/// </summary>
internal partial record ClassModel : IClassModel
{
    /// <inheritdoc/>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the field table.
    /// </summary>
    required public ReadOnlyFieldTable FieldTable { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IField> GetFields()
    {
        List<IField> Result = new();

        foreach (KeyValuePair<FieldName, Field> Entry in FieldTable)
            Result.Add(Entry.Value);

        return Result.AsReadOnly();
    }

    /// <summary>
    /// Gets the method table.
    /// </summary>
    required public ReadOnlyMethodTable MethodTable { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IMethod> GetMethods()
    {
        List<IMethod> Result = new();

        foreach (KeyValuePair<MethodName, Method> Entry in MethodTable)
            Result.Add(Entry.Value);

        return Result.AsReadOnly();
    }

    /// <summary>
    /// Gets the list of invariants.
    /// </summary>
    required public List<Invariant> InvariantList { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IInvariant> GetInvariants()
    {
        List<IInvariant> Result = new();

        foreach (Invariant Item in InvariantList)
            Result.Add(Item);

        return Result.AsReadOnly();
    }

    /// <inheritdoc/>
    required public Unsupported Unsupported { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IInvariantViolation> InvariantViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IRequireViolation> RequireViolations { get; init; }

    /// <inheritdoc/>
    required public IReadOnlyList<IEnsureViolation> EnsureViolations { get; init; }

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
    /// <param name="fieldTable">The table of fields.</param>
    /// <param name="parameterTable">The table of parameters.</param>
    /// <param name="resultField">The optional result field.</param>
    /// <param name="variableName">The variable name.</param>
    public static IVariable GetVariable(ReadOnlyFieldTable fieldTable, ReadOnlyParameterTable parameterTable, Field? resultField, IVariableName variableName)
    {
        IVariable? Result = null;

        foreach (KeyValuePair<FieldName, Field> Entry in fieldTable)
            if (Entry.Key.Text == variableName.Text)
            {
                Result = Entry.Value;
                break;
            }

        foreach (KeyValuePair<ParameterName, Parameter> Entry in parameterTable)
            if (Entry.Key.Text == variableName.Text)
            {
                Result = Entry.Value;
                break;
            }

        if (resultField is not null && resultField.Name.Text == variableName.Text)
        {
            Debug.Assert(Result is null);

            Result = resultField;
        }

        Debug.Assert(Result is not null);

        return Result!;
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
            string TypeString = ExpressionTypeToString(Field.Type);

            string InitializerString;
            ILiteralExpression? Initializer = Field.Initializer;

            if (Initializer is not null)
                InitializerString = $" = {Initializer}";
            else
                InitializerString = string.Empty;

            builder.AppendLine($"  {TypeString} {Field.Name.Text}{InitializerString}");
        }
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
        AppendStatements(builder, statement.WhenTrueStatementList, indentation);

        if (statement.WhenFalseStatementList.Count > 0)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("else");
            AppendStatements(builder, statement.WhenFalseStatementList, indentation);
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
