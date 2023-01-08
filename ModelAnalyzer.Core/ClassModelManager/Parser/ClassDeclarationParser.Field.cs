namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private FieldTable ParseFields(ParsingContext parsingContext, ClassDeclarationSyntax classDeclaration)
    {
        FieldTable FieldTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is FieldDeclarationSyntax FieldDeclaration)
                AddField(parsingContext, FieldTable, FieldDeclaration);

        return FieldTable;
    }

    private void AddField(ParsingContext parsingContext, FieldTable fieldTable, FieldDeclarationSyntax fieldDeclaration)
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

        if (!IsTypeSupported(parsingContext, Declaration.Type, out ExpressionType FieldType))
        {
            LogWarning($"Unsupported field type.");

            IsFieldSupported = false;
        }

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
            AddField(parsingContext, fieldTable, Variable, IsFieldSupported, FieldType);
    }

    private void AddField(ParsingContext parsingContext, FieldTable fieldTable, VariableDeclaratorSyntax variable, bool isFieldSupported, ExpressionType fieldType)
    {
        string FieldName = variable.Identifier.ValueText;
        FieldName Name = new() { Text = FieldName };
        bool IsErrorReported = false;

        // Ignore duplicate names, the compiler will catch them.
        if (!fieldTable.ContainsItem(Name))
        {
            bool IsFieldSupported = isFieldSupported; // Initialize with the result of previous checks (type etc.)
            ILiteralExpression? Initializer = null;

            if (FieldName == Ensure.ResultKeyword)
            {
                LogWarning($"Unsupported field name {Ensure.ResultKeyword}.");

                IsFieldSupported = false;
            }

            if (variable.Initializer is EqualsValueClauseSyntax EqualsValueClause)
            {
                if (!TryParseInitializerNode(parsingContext, EqualsValueClause, fieldType, out Initializer))
                {
                    IsFieldSupported = false;
                    IsErrorReported = true;
                }
            }

            if (IsFieldSupported)
            {
                Field NewField = new Field { Name = Name, Type = fieldType, Initializer = Initializer };
                fieldTable.AddItem(NewField);
            }
            else if (!IsErrorReported)
            {
                Location Location = variable.Identifier.GetLocation();
                parsingContext.Unsupported.AddUnsupportedField(Name, Location);
            }
        }
    }

    private bool TryFindFieldByName(ParsingContext parsingContext, string fieldName, out IField field)
    {
        foreach (KeyValuePair<FieldName, Field> Entry in parsingContext.FieldTable)
            if (Entry.Value.Name.Text == fieldName)
            {
                field = Entry.Value;
                return true;
            }

        field = null!;
        return false;
    }
}
