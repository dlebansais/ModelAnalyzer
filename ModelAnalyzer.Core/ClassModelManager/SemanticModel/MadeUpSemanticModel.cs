namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a made up semantic model.
/// </summary>
public class MadeUpSemanticModel : IModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MadeUpSemanticModel"/> class.
    /// </summary>
    public MadeUpSemanticModel()
    {
    }

    /// <inheritdoc/>
    public bool HasBaseType(ClassDeclarationSyntax classDeclaration)
    {
        bool Result = false;

        if (classDeclaration.BaseList is BaseListSyntax BaseList && BaseList.Types.Count > 0)
        {
            // We only tolerate base types that start with a 'I', indicating a probable interface.
            // Only test classes need this, real world classes will have a semantic model available, but it helps with tests.
            foreach (BaseTypeSyntax Item in BaseList.Types)
            {
                SimpleBaseTypeSyntax SimpleBaseType = (SimpleBaseTypeSyntax)Item;
                SimpleNameSyntax? SimpleName = SimpleBaseType.Type as SimpleNameSyntax;

                if (SimpleName?.Identifier is not SyntaxToken Identifier || !Identifier.ValueText.StartsWith("I"))
                    Result = true;
            }
        }

        return Result;
    }

    /// <inheritdoc/>
    public bool GetClassType(IdentifierNameSyntax identifierName, List<ClassDeclarationSyntax> classDeclarationList, bool isNullable, out ExpressionType classType)
    {
        string ClassName = identifierName.Identifier.ValueText;

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
            if (ClassDeclaration.Identifier.ValueText == ClassName)
            {
                classType = new ExpressionType(ClassName, isNullable);
                return true;
            }

        classType = ExpressionType.Other;
        return false;
    }
}
