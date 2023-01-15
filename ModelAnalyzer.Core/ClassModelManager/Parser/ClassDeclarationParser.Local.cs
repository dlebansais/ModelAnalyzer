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

        Debug.Assert(parsingContext.HostMethod is not null);
        Method HostMethod = parsingContext.HostMethod!;

        Local? ResultLocal = LocalTable.GetResultLocal();

        if (ResultLocal is null)
        {
            ExpressionType ReturnType = HostMethod.ReturnType;
            Debug.Assert(ReturnType != ExpressionType.Other);

            if (HostMethod.ReturnType != ExpressionType.Void)
                HostMethod.ResultLocal = new Local() { Name = new LocalName() { Text = Ensure.ResultKeyword }, Type = ReturnType, Initializer = null };
        }

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

        if (!IsTypeSupported(parsingContext, Declaration.Type, out ExpressionType LocalType))
        {
            LogWarning($"Unsupported local type.");

            IsLocalSupported = false;
        }

        foreach (VariableDeclaratorSyntax Variable in Declaration.Variables)
            AddLocal(parsingContext, localTable, Variable, IsLocalSupported, LocalType);
    }

    private void AddLocal(ParsingContext parsingContext, LocalTable localTable, VariableDeclaratorSyntax variable, bool isLocalSupported, ExpressionType localType)
    {
        string LocalName = variable.Identifier.ValueText;
        LocalName Name = new() { Text = LocalName };
        bool IsErrorReported = false;

        // Ignore duplicate names, the compiler will catch them.
        if (!localTable.ContainsItem(Name))
        {
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

            if (IsLocalSupported)
            {
                Local NewLocal = new Local { Name = Name, Type = localType, Initializer = Initializer };
                localTable.AddItem(NewLocal);
            }
            else if (!IsErrorReported)
            {
                Location Location = variable.Identifier.GetLocation();
                parsingContext.Unsupported.AddUnsupportedLocal(Location);
            }
        }
    }

    private bool TryFindLocalByName(ParsingContext parsingContext, string localName, out ILocal local)
    {
        Debug.Assert(parsingContext.HostMethod is not null);

        Method HostMethod = parsingContext.HostMethod!;

        foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
            if (Entry.Value.Name.Text == localName)
            {
                local = Entry.Value;
                return true;
            }

        local = null!;
        return false;
    }
}
