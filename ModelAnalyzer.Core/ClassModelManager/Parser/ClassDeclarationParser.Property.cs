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
    private PropertyTable ParseProperties(ParsingContext parsingContext, ClassDeclarationSyntax classDeclaration)
    {
        PropertyTable PropertyTable = new();

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
            if (Member is PropertyDeclarationSyntax PropertyDeclaration)
                AddProperty(parsingContext, PropertyTable, PropertyDeclaration);

        return PropertyTable;
    }

    private void AddProperty(ParsingContext parsingContext, PropertyTable propertyTable, PropertyDeclarationSyntax propertyDeclaration)
    {
        bool IsPropertySupported = true;
        ILiteralExpression? Initializer = null;
        bool IsErrorReported = false;
        string PropertyName = propertyDeclaration.Identifier.ValueText;
        PropertyName Name = new() { Text = PropertyName };

        // Ignore duplicate names, the compiler will catch them.
        if (propertyTable.ContainsItem(Name))
            return;

        if (!IsPropertyDeclarationSupported(propertyDeclaration))
            IsPropertySupported = false;

        if (!IsTypeSupported(parsingContext, propertyDeclaration.Type, out ExpressionType PropertyType, out _))
        {
            LogWarning($"Unsupported property type.");

            IsPropertySupported = false;
        }

        if (propertyDeclaration.AccessorList is null)
        {
            LogWarning($"Unsupported property without accessors.");

            IsPropertySupported = false;
        }
        else
        {
            AccessorListSyntax AccessorList = propertyDeclaration.AccessorList;

            SyntaxList<AccessorDeclarationSyntax> Accessors = AccessorList.Accessors;

            if (Accessors.Count != 2)
            {
                LogWarning($"Unsupported property without both get and set accessors.");

                IsPropertySupported = false;
            }
            else
            {
                AccessorDeclarationSyntax GetAccessor = Accessors[0];
                AccessorDeclarationSyntax SetAccessor = Accessors[1];

                if (!IsValidAccessor(GetAccessor, SyntaxKind.GetKeyword))
                    IsPropertySupported = false;

                if (!IsValidAccessor(SetAccessor, SyntaxKind.SetKeyword))
                    IsPropertySupported = false;

                if (propertyDeclaration.Initializer is EqualsValueClauseSyntax EqualsValueClause)
                {
                    if (!TryParseInitializerNode(parsingContext, EqualsValueClause, PropertyType, out Initializer))
                    {
                        IsPropertySupported = false;
                        IsErrorReported = true;
                    }
                }
            }
        }

        if (IsPropertySupported)
        {
            Property NewProperty = new Property { Name = Name, Type = PropertyType, Initializer = Initializer, ClassName = parsingContext.ClassName };
            propertyTable.AddItem(NewProperty);
        }
        else if (!IsErrorReported)
        {
            Location Location = propertyDeclaration.Identifier.GetLocation();
            parsingContext.Unsupported.AddUnsupportedProperty(Location, parsingContext.ClassName);
        }
    }

    private bool IsPropertyDeclarationSupported(PropertyDeclarationSyntax propertyDeclaration)
    {
        string PropertyName = propertyDeclaration.Identifier.ValueText;

        if (PropertyName == Ensure.ResultKeyword)
        {
            LogWarning($"Unsupported property name {Ensure.ResultKeyword}.");

            return false;
        }

        if (propertyDeclaration.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {propertyDeclaration.AttributeLists.Count} property attribute(s).");

            return false;
        }

        if (propertyDeclaration.ExplicitInterfaceSpecifier is not null)
        {
            LogWarning($"Unsupported property interface.");

            return false;
        }

        bool IsPublic = false;

        foreach (SyntaxToken Modifier in propertyDeclaration.Modifiers)
            if (!Modifier.IsKind(SyntaxKind.PublicKeyword) && !Modifier.IsKind(SyntaxKind.InternalKeyword))
            {
                LogWarning($"Unsupported '{Modifier.ValueText}' property modifier.");

                return false;
            }
            else
                IsPublic = true;

        if (!IsPublic)
        {
            LogWarning($"Unsupported non-public property.");

            return false;
        }

        if (propertyDeclaration.ExpressionBody is not null)
        {
            LogWarning($"Unsupported property expression body.");

            return false;
        }

        return true;
    }

    private bool IsValidAccessor(AccessorDeclarationSyntax accessor, SyntaxKind expectedKeyword)
    {
        if (accessor.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {accessor.AttributeLists.Count} property accessor attribute(s).");

            return false;
        }

        if (accessor.Modifiers.Count > 0)
        {
            foreach (SyntaxToken Modifier in accessor.Modifiers)
                LogWarning($"Unsupported '{Modifier.ValueText}' property accessor modifier.");

            return false;
        }

        if (!accessor.Keyword.IsKind(expectedKeyword))
        {
            LogWarning($"Expected property accessor '{expectedKeyword}'.");

            return false;
        }

        if (accessor.Body is not null)
        {
            LogWarning($"Unsupported property accessor body.");

            return false;
        }

        if (accessor.ExpressionBody is not null)
        {
            LogWarning($"Unsupported property accessor expression body.");

            return false;
        }

        return true;
    }

    private bool TryFindPropertyByName(ParsingContext parsingContext, string propertyName, out IProperty property)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in parsingContext.PropertyTable)
            if (Entry.Value.Name.Text == propertyName)
            {
                property = Entry.Value;
                return true;
            }

        property = null!;
        return false;
    }
}
