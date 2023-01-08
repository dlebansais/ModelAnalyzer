namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides information about a semantic model.
/// </summary>
public interface IModel
{
    /// <summary>
    /// Checks whether a class has a base class.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    bool HasBaseType(ClassDeclarationSyntax classDeclaration);

    /// <summary>
    /// Gets the type corresponding to the name provided.
    /// </summary>
    /// <param name="identifierName">The name.</param>
    /// <param name="classDeclarationList">The list of class declarations.</param>
    /// <param name="classType">The type upon return if a type with that name exists.</param>
    bool GetClassType(IdentifierNameSyntax identifierName, List<ClassDeclarationSyntax> classDeclarationList, out ExpressionType classType);
}
