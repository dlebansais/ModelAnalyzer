namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private ReadOnlyLocalTable ParseLocals(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported)
    {
        LocalTable LocalTable;

        if (methodDeclaration.Body is BlockSyntax Block)
            LocalTable = ParseBlockLocals(fieldTable, hostMethod, unsupported, Block);
        else
            LocalTable = new();

        return LocalTable.ToReadOnly();
    }

    private LocalTable ParseBlockLocals(ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported, BlockSyntax block)
    {
        LocalTable LocalTable = new();

        foreach (StatementSyntax Item in block.Statements)
            if (Item is LocalDeclarationStatementSyntax LocalDeclarationStatement)
                AddLocal(LocalDeclarationStatement, LocalTable, fieldTable, hostMethod, unsupported);

        return LocalTable;
    }

    private void AddLocal(LocalDeclarationStatementSyntax localDeclarationStatement, LocalTable localTable, ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported)
    {
        VariableDeclarationSyntax Declaration = localDeclarationStatement.Declaration;
        bool IsLocalSupported = true;

        if (localDeclarationStatement.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {localDeclarationStatement.AttributeLists.Count} local attribute(s).");

            IsLocalSupported = false;
        }

        foreach (SyntaxToken Modifier in localDeclarationStatement.Modifiers)
        {
            LogWarning($"Unsupported '{Modifier.ValueText}' local modifier.");

            IsLocalSupported = false;
        }

        if (!IsTypeSupported(Declaration.Type, out ExpressionType LocalType))
        {
            LogWarning($"Unsupported local type.");

            IsLocalSupported = false;
        }

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
            AddLocal(Variable, localTable, fieldTable, hostMethod, unsupported, IsLocalSupported, LocalType);
    }

    private void AddLocal(VariableDeclaratorSyntax variable, LocalTable localTable, ReadOnlyFieldTable fieldTable, Method hostMethod, Unsupported unsupported, bool isLocalSupported, ExpressionType localType)
    {
        string LocalName = variable.Identifier.ValueText;
        LocalName Name = new() { Text = LocalName };
        bool IsErrorReported = false;

        // Ignore duplicate names, the compiler will catch them.
        if (!localTable.ContainsItem(Name))
        {
            bool IsLocalSupported = isLocalSupported; // Initialize with the result of previous checks (type etc.)
            ILiteralExpression? Initializer = null;

            if (LocalName == Ensure.ResultKeyword)
                IsLocalSupported = false;

            if (variable.Initializer is EqualsValueClauseSyntax EqualsValueClause)
            {
                if (!TryParseInitializerNode(unsupported, EqualsValueClause, localType, out Initializer))
                {
                    IsLocalSupported = false;
                    IsErrorReported = true;
                }
            }

            if (TryFindFieldByName(fieldTable, LocalName, out _))
            {
                LogWarning($"Local '{LocalName}' is already the name of a field.");

                IsLocalSupported = false;
            }
            else if (TryFindParameterByName(hostMethod.ParameterTable, LocalName, out _))
            {
                LogWarning($"Local '{LocalName}' is already the name of a parameter.");

                IsLocalSupported = false;
            }

            if (IsLocalSupported)
            {
                Local NewLocal = new Local { Name = Name, Type = localType, Initializer = Initializer };
                localTable.AddItem(NewLocal);
            }
            else if (!IsErrorReported)
            {
                Location Location = variable.Identifier.GetLocation();
                unsupported.AddUnsupportedLocal(Location);
            }
        }
    }

    private bool TryFindLocalByName(ReadOnlyLocalTable localTable, string localName, out ILocal local)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in localTable)
            if (Entry.Value.Name.Text == localName)
            {
                local = Entry.Value;
                return true;
            }

        local = null!;
        return false;
    }
}
