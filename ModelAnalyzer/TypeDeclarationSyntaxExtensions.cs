namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class TypeDeclarationSyntaxExtensions
{
    private const char NestClassDelimiter = '+';
    private const char NamespaceClassDelimiter = '.';
    private const char TypeParameterClassDelimiter = '`';

    public static string GetFullName(this TypeDeclarationSyntax source)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var namespaces = new LinkedList<BaseNamespaceDeclarationSyntax>();
        var types = new LinkedList<TypeDeclarationSyntax>();

        for (var parent = source.Parent; parent is object; parent = parent.Parent)
        {
            if (parent is BaseNamespaceDeclarationSyntax @namespace)
            {
                namespaces.AddFirst(@namespace);
            }
            else if (parent is TypeDeclarationSyntax type)
            {
                types.AddFirst(type);
            }
        }

        var result = new StringBuilder();

        for (var item = namespaces.First; item is object; item = item.Next)
        {
            result.Append(item.Value.Name).Append(NamespaceClassDelimiter);
        }

        for (var item = types.First; item is object; item = item.Next)
        {
            var type = item.Value;
            AppendName(result, type);
            result.Append(NestClassDelimiter);
        }

        AppendName(result, source);

        return result.ToString();
    }

    private static void AppendName(StringBuilder builder, TypeDeclarationSyntax type)
    {
        builder.Append(type.Identifier.Text);

        var typeArguments = type.TypeParameterList?.ChildNodes().Count(node => node is TypeParameterSyntax) ?? 0;

        if (typeArguments != 0)
            builder.Append(TypeParameterClassDelimiter).Append(typeArguments);
    }
}
