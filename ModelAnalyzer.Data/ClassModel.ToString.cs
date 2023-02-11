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
    public override string ToString()
    {
        StringBuilder Builder = new();

        AppendClassName(Builder);
        AppendProperties(Builder);
        AppendFields(Builder);
        AppendMethods(Builder);
        AppendInvariants(Builder);

        return TextBuilder.Normalized(Builder.ToString());
    }

    private void AppendClassName(StringBuilder builder)
    {
        builder.AppendLine(ClassName.Text);
    }

    private void AppendProperties(StringBuilder builder)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in PropertyTable)
            AppendProperty(builder, Entry.Value);
    }

    private void AppendProperty(StringBuilder builder, Property property)
    {
        string TypeString = ExpressionTypeToString(property.Type);

        string InitializerString;
        ILiteralExpression? Initializer = property.Initializer;

        if (Initializer is not null)
            InitializerString = $" = {Initializer}";
        else
            InitializerString = string.Empty;

        builder.AppendLine($"  public {TypeString} {property.Name.Text} {{ get; set; }}{InitializerString}");
    }

    private void AppendFields(StringBuilder builder)
    {
        if (!PropertyTable.IsEmpty && !FieldTable.IsEmpty)
            builder.AppendLine();

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
            if (!IsFirst || !PropertyTable.IsEmpty || !FieldTable.IsEmpty)
                builder.AppendLine();

            MethodToString(builder, MethodEntry.Value);
            IsFirst = false;
        }
    }

    private void MethodToString(StringBuilder builder, Method method)
    {
        string Modifier = method.AccessModifier == AccessModifier.Public ? "public " : string.Empty;
        Modifier += method.IsStatic ? "static " : string.Empty;

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

        BlockScope RootBlock = method.RootBlock;

        AppendLocals(builder, RootBlock.LocalTable);

        if (RootBlock.LocalTable.Count > 0 && RootBlock.StatementList.Count > 0)
            builder.AppendLine();

        AppendStatements(builder, RootBlock, 0);

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

    private void AppendBlock(StringBuilder builder, BlockScope block, int indentation)
    {
        if (block.StatementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("{");
        }

        AppendStatements(builder, block, indentation);

        if (block.StatementList.Count > 1)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("}");
        }
    }

    private void AppendStatements(StringBuilder builder, BlockScope block, int indentation)
    {
        foreach (Statement Item in block.StatementList)
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
            case PrivateMethodCallStatement MethodCall:
                AppendPrivateMethodCallStatement(builder, MethodCall, indentation);
                IsHandled = true;
                break;
            case PublicMethodCallStatement MethodCall:
                AppendPublicMethodCallStatement(builder, MethodCall, indentation);
                IsHandled = true;
                break;
            case ReturnStatement Return:
                AppendReturnStatement(builder, Return, indentation);
                IsHandled = true;
                break;
            case ForLoopStatement ForLoop:
                AppendForLoopStatement(builder, ForLoop, indentation);
                IsHandled = true;
                break;
        }

        Debug.Assert(IsHandled);
    }

    private void AppendAssignmentStatement(StringBuilder builder, AssignmentStatement statement, int indentation)
    {
        if (statement.DestinationIndex is not null)
            AppendStatementText(builder, $"{statement.DestinationName.Text}[{statement.DestinationIndex}] = {statement.Expression}", indentation);
        else
            AppendStatementText(builder, $"{statement.DestinationName.Text} = {statement.Expression}", indentation);
    }

    private void AppendConditionalStatement(StringBuilder builder, ConditionalStatement statement, int indentation)
    {
        AppendIndentation(builder, indentation);
        builder.AppendLine($"if ({statement.Condition})");
        AppendBlock(builder, statement.WhenTrueBlock, indentation);

        if (statement.WhenFalseBlock.StatementList.Count > 0)
        {
            AppendIndentation(builder, indentation);
            builder.AppendLine("else");
            AppendBlock(builder, statement.WhenFalseBlock, indentation);
        }
    }

    private void AppendPrivateMethodCallStatement(StringBuilder builder, PrivateMethodCallStatement statement, int indentation)
    {
        string StaticString = statement.ClassName == ClassName.Empty ? string.Empty : $"{statement.ClassName}.";
        AppendStatementText(builder, $"{StaticString}{statement.MethodName.Text}({string.Join(", ", statement.ArgumentList)})", indentation);
    }

    private void AppendPublicMethodCallStatement(StringBuilder builder, PublicMethodCallStatement statement, int indentation)
    {
        List<string> NamePath;

        if (statement.IsStatic)
            NamePath = statement.ClassName.ToNamePath();
        else
            NamePath = statement.VariablePath.ConvertAll(item => item.Name.Text);

        AppendStatementText(builder, $"{string.Join(".", NamePath)}.{statement.MethodName.Text}({string.Join(", ", statement.ArgumentList)})", indentation);
    }

    private void AppendReturnStatement(StringBuilder builder, ReturnStatement statement, int indentation)
    {
        if (statement.Expression is null)
            AppendStatementText(builder, "return", indentation);
        else
            AppendStatementText(builder, $"return {statement.Expression}", indentation);
    }

    private void AppendForLoopStatement(StringBuilder builder, ForLoopStatement statement, int indentation)
    {
        AppendIndentation(builder, indentation);

        string IndexName = statement.LocalIndex.Name.Text;
        ILiteralExpression? Initializer = statement.LocalIndex.Initializer;
        string InitializerString = Initializer is not null ? $" = {Initializer}" : string.Empty;

        builder.AppendLine($"for (int {IndexName}{InitializerString}; {statement.ContinueCondition}; {IndexName}++)");
        AppendBlock(builder, statement.Block, indentation);
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
        Debug.Assert(expressionType != ExpressionType.Other);

        string NameString = expressionType.TypeName.ToString();
        string NullableString = expressionType.IsNullable ? "?" : string.Empty;
        string ArrayString = expressionType.IsArray ? "[]" : string.Empty;

        return $"{NameString}{NullableString}{ArrayString}";
    }
}
