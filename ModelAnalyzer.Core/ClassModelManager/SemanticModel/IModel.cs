namespace ModelAnalyzer;

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
}
