﻿namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a semantic model from an analyzer.
/// </summary>
public class AnalyzerSemanticModel : IModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzerSemanticModel"/> class.
    /// </summary>
    /// <param name="semanticModel">The semantic model from an analyzer.</param>
    public AnalyzerSemanticModel(SemanticModel semanticModel)
    {
        SemanticModel = semanticModel;
    }

    /// <summary>
    /// Gets the semantic model.
    /// </summary>
    public SemanticModel SemanticModel { get; }

    /// <inheritdoc/>
    public bool HasBaseType(ClassDeclarationSyntax classDeclaration)
    {
        INamedTypeSymbol? TypeSymbol = SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        INamedTypeSymbol? BaseType = TypeSymbol?.BaseType;

        // BaseType is null only if the base is 'object', meaning there is no base.
        return BaseType?.BaseType is not null;
    }

    /// <inheritdoc/>
    public bool GetClassType(IdentifierNameSyntax identifierName, List<ClassDeclarationSyntax> classDeclarationList, out ExpressionType classType)
    {
        SymbolInfo SymbolInfo = SemanticModel.GetSymbolInfo(identifierName);

        if (SymbolInfo.Symbol is INamedTypeSymbol NamedTypeSymbol)
        {
            if (NamedTypeSymbol.TypeKind == TypeKind.Class)
            {
                string ClassName = NamedTypeSymbol.Name;

                foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
                    if (ClassDeclaration.Identifier.ValueText == ClassName)
                    {
                        classType = new ExpressionType(ClassName);
                        return true;
                    }
            }
        }

        classType = ExpressionType.Other;
        return false;
    }
}
