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

        if (!IsTypeSupported(Declaration.Type, out _))
        {
            LogWarning($"Unsupported field type.");

            IsFieldSupported = false;
        }

        if (!IsFieldSupported)
        {
            Location Location = Declaration.GetLocation();
            unsupported.AddUnsupportedField(Location, out _);
        }
        else
        {
            foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
                AddField(Variable, fieldTable, unsupported);
        }
    }

    private void AddField(VariableDeclaratorSyntax variable, FieldTable fieldTable, Unsupported unsupported)
    {
        FieldName FieldName = new(variable.Identifier.ValueText);

        // Ignore duplicate names, the compiler will catch them.
        if (!fieldTable.ContainsItem(FieldName))
        {
            IField NewField;
            bool IsFieldSupported = true;

            if (variable.ArgumentList is BracketedArgumentListSyntax BracketedArgumentList && BracketedArgumentList.Arguments.Count > 0)
            {
                LogWarning($"Unsupported bracketed syntax for field.");

                IsFieldSupported = false;
            }

            if (variable.Initializer is not null)
            {
                LogWarning($"Unsupported field initializer.");

                IsFieldSupported = false;
            }

            if (IsFieldSupported)
            {
                NewField = new Field { FieldName = FieldName };
            }
            else
            {
                Location Location = variable.Identifier.GetLocation();
                unsupported.AddUnsupportedField(Location, out IUnsupportedField UnsupportedField);

                NewField = UnsupportedField;
            }

            fieldTable.AddItem(FieldName, NewField);
        }
    }

    private bool TryFindFieldByName(FieldTable fieldTable, string fieldName, out IField field)
    {
        foreach (KeyValuePair<FieldName, IField> Entry in fieldTable)
            if (Entry.Value is Field ValidField && ValidField.FieldName.Name == fieldName)
            {
                field = ValidField;
                return true;
            }

        field = null!;
        return false;
    }
}
