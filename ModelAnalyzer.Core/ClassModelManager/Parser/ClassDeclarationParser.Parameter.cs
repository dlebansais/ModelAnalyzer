namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private ParameterTable ParseParameters(ParsingContext parsingContext, MethodDeclarationSyntax methodDeclaration)
    {
        ParameterTable ParameterTable = new();

        foreach (ParameterSyntax Parameter in methodDeclaration.ParameterList.Parameters)
        {
            ParameterName ParameterName = new() { Text = Parameter.Identifier.ValueText };

            // Ignore duplicate names, the compiler will catch them.
            if (!ParameterTable.ContainsItem(ParameterName))
            {
                if (IsParameterSupported(parsingContext, Parameter, out ExpressionType ParameterType))
                {
                    Parameter NewParameter = new Parameter() { Name = ParameterName, Type = ParameterType };
                    ParameterTable.AddItem(NewParameter);
                }
                else if (!parsingContext.IsMethodParsingFirstPassDone)
                {
                    Location Location = Parameter.GetLocation();
                    parsingContext.Unsupported.AddUnsupportedParameter(Location);
                }
            }
        }

        return ParameterTable;
    }

    private bool IsParameterSupported(ParsingContext parsingContext, ParameterSyntax parameter, out ExpressionType parameterType)
    {
        bool IsParameterSupported = true;

        if (parameter.AttributeLists.Count > 0)
        {
            LogWarning($"Unsupported {parameter.AttributeLists.Count} parameter attribute(s).");

            IsParameterSupported = false;
        }

        if (parameter.Modifiers.Count > 0)
        {
            LogWarning($"Unsupported {parameter.Modifiers.Count} parameter modifier(s).");

            IsParameterSupported = false;
        }

        if (!IsTypeSupported(parsingContext, parameter.Type, out parameterType, out _))
        {
            LogWarning($"Unsupported parameter type.");

            IsParameterSupported = false;
        }

        string ParameterName = parameter.Identifier.ValueText;

        if (ParameterName == Ensure.ResultKeyword)
        {
            LogWarning($"Parameter '{ParameterName}' is not allowed.");

            IsParameterSupported = false;
        }

        if (TryFindPropertyByName(parsingContext, ParameterName, out _))
        {
            LogWarning($"Parameter '{ParameterName}' is already the name of a property.");

            IsParameterSupported = false;
        }
        else if (TryFindFieldByName(parsingContext, ParameterName, out _))
        {
            LogWarning($"Parameter '{ParameterName}' is already the name of a field.");

            IsParameterSupported = false;
        }

        return IsParameterSupported;
    }

    private bool TryFindParameterByName(ParsingContext parsingContext, string parameterName, out IParameter parameter)
    {
        if (parsingContext.HostMethod is not null)
        {
            foreach (KeyValuePair<ParameterName, Parameter> Entry in parsingContext.HostMethod.ParameterTable)
                if (Entry.Value.Name.Text == parameterName)
                {
                    parameter = Entry.Value;
                    return true;
                }
        }

        parameter = null!;
        return false;
    }
}
