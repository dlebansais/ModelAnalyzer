namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private LocalTable ParseLocals(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration)
    {
        LocalTable LocalTable;

        if (methodDeclaration.Body is BlockSyntax Block)
            LocalTable = ParseBlockLocals(parsingContext, Block);
        else
            LocalTable = new();

        return LocalTable;
    }

    private LocalTable ParseBlockLocals(ParsingContext parsingContext, BlockSyntax block)
    {
        LocalTable LocalTable = new();

        foreach (StatementSyntax Item in block.Statements)
            if (Item is LocalDeclarationStatementSyntax LocalDeclarationStatement)
                AddLocal(parsingContext, LocalTable, LocalDeclarationStatement);

        return LocalTable;
    }

    private void AddLocal(ParsingContext parsingContext, LocalTable localTable, LocalDeclarationStatementSyntax localDeclarationStatement)
    {
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

        VariableDeclarationSyntax Declaration = localDeclarationStatement.Declaration;
        AddLocal(parsingContext, localTable, Declaration, IsLocalSupported);
    }

    private void AddLocal(ParsingContext parsingContext, LocalTable localTable, VariableDeclarationSyntax declaration, bool isLocalSupported)
    {
        if (!IsTypeSupported(parsingContext, declaration.Type, out ExpressionType LocalType, out _))
        {
            LogWarning($"Unsupported local type.");

            isLocalSupported = false;
        }

        foreach (VariableDeclaratorSyntax Variable in declaration.Variables)
            AddLocal(parsingContext, localTable, Variable, isLocalSupported, LocalType);
    }

    private bool AddLocal(ParsingContext parsingContext, LocalTable localTable, VariableDeclaratorSyntax variable, bool isLocalSupported, ExpressionType localType)
    {
        string LocalName = variable.Identifier.ValueText;
        LocalName Name = new() { Text = LocalName };
        bool IsErrorReported = false;

        // Ignore duplicate names, the compiler will catch them.
        if (localTable.ContainsItem(Name))
            return false;

        bool IsLocalSupported = isLocalSupported; // Initialize with the result of previous checks (type etc.)
        ILiteralExpression? Initializer = null;

        if (variable.Initializer is EqualsValueClauseSyntax EqualsValueClause)
        {
            if (!TryParseInitializerNode(parsingContext, EqualsValueClause, localType, out Initializer))
            {
                IsLocalSupported = false;
                IsErrorReported = true;
            }
        }

        if (TryFindPropertyByName(parsingContext, LocalName, out _))
        {
            LogWarning($"Local '{LocalName}' is already the name of a property.");

            IsLocalSupported = false;
        }
        else if (TryFindFieldByName(parsingContext, LocalName, out _))
        {
            LogWarning($"Local '{LocalName}' is already the name of a field.");

            IsLocalSupported = false;
        }
        else if (TryFindParameterByName(parsingContext, LocalName, out _))
        {
            LogWarning($"Local '{LocalName}' is already the name of a parameter.");

            IsLocalSupported = false;
        }

        if (!IsLocalSupported)
        {
            if (!IsErrorReported)
            {
                Location Location = variable.Identifier.GetLocation();
                parsingContext.Unsupported.AddUnsupportedLocal(Location);
            }

            return false;
        }

        Local NewLocal = new Local { Name = Name, Type = localType, Initializer = Initializer };
        localTable.AddItem(NewLocal);

        return true;
    }

    private bool TryFindLocalByName(ParsingContext parsingContext, string localName, out ILocal local)
    {
        Debug.Assert(parsingContext.HostBlock is not null);

        BlockScope HostBlock = parsingContext.HostBlock!;

        foreach (KeyValuePair<LocalName, Local> Entry in HostBlock.LocalTable)
            if (Entry.Value.Name.Text == localName)
            {
                local = Entry.Value;
                return true;
            }

        local = null!;
        return false;
    }
}
