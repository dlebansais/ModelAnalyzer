namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private FieldTable ParseFields(ClassDeclarationSyntax classDeclaration, Unsupported unsupported)
    {
        FieldTable FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(FieldDeclaration, FieldTable, unsupported);

        FieldTable.Seal();

        return FieldTable;
    }

    private void AddField(FieldDeclarationSyntax fieldDeclaration, FieldTable fieldTable, Unsupported unsupported)
    {
        VariableDeclarationSyntax Declaration = fieldDeclaration.Declaration;
        bool IsFieldSupported = true;

        if (fieldDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {fieldDeclaration.AttributeLists.Count} field attribute(s).");

            IsFieldSupported = false;
        }

        foreach (SyntaxToken Modifier in fieldDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PrivateKeyword))
            {
                LogWarning($"Unsupported '{Modifier.ValueText}' field modifier.");

                IsFieldSupported = false;
            }

        if (!IsTypeSupported(Declaration.Type, out ExpressionType FieldType))
        {
            LogWarning($"Unsupported field type.");

            IsFieldSupported = false;
        }

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
            AddField(Variable, fieldTable, unsupported, IsFieldSupported, FieldType);
    }

    private void AddField(VariableDeclaratorSyntax variable, FieldTable fieldTable, Unsupported unsupported, bool isFieldSupported, ExpressionType fieldType)
    {
        FieldName FieldName = new() { Name = variable.Identifier.ValueText };
        bool IsErrorReported = false;

        // Ignore duplicate names, the compiler will catch them.
        if (!fieldTable.ContainsItem(FieldName))
        {
            bool IsFieldSupported = isFieldSupported; // Initialize with the result of previous checks (type etc.)
            Expression? Initializer = null;

            if (variable.Initializer is EqualsValueClauseSyntax EqualsValueClause)
            {
                if (!TryParseFieldInitializer(unsupported, EqualsValueClause, fieldType, out Initializer))
                {
                    IsFieldSupported = false;
                    IsErrorReported = true;
                }
            }

            if (IsFieldSupported)
            {
                Field NewField = new Field { FieldName = FieldName, VariableType = fieldType, Initializer = Initializer };
                fieldTable.AddItem(FieldName, NewField);
            }
            else if (!IsErrorReported)
            {
                Location Location = variable.Identifier.GetLocation();
                unsupported.AddUnsupportedField(Location);
            }
        }
    }

    private bool TryFindFieldByName(FieldTable fieldTable, string fieldName, out IField field)
    {
        foreach (KeyValuePair<FieldName, Field> Entry in fieldTable)
            if (Entry.Value.FieldName.Name == fieldName)
            {
                field = Entry.Value;
                return true;
            }

        field = null!;
        return false;
    }

    private bool TryParseFieldInitializer(Unsupported unsupported, EqualsValueClauseSyntax equalsValueClause, ExpressionType fieldType, out Expression? initializerExpression)
    {
        ExpressionSyntax InitializerValue = equalsValueClause.Value;

        if (InitializerValue is LiteralExpressionSyntax literalExpression)
        {
            Expression? ParsedExpression = TryParseLiteralValueExpression(literalExpression);

            if (fieldType == ExpressionType.Boolean && ParsedExpression is LiteralBoolValueExpression BooleanExpression)
            {
                initializerExpression = BooleanExpression;
                return true;
            }
            else if (fieldType == ExpressionType.Integer && ParsedExpression is LiteralIntValueExpression IntegerExpression)
            {
                initializerExpression = IntegerExpression;
                return true;
            }
        }

        LogWarning("Unsupported field initializer.");

        Location Location = InitializerValue.GetLocation();
        unsupported.AddUnsupportedExpression(Location);
        initializerExpression = null;
        return false;
    }
}
