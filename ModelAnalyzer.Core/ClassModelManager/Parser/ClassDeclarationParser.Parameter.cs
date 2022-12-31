namespace ModelAnalyzer;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a class declaration parser.
/// </summary>
internal partial class ClassDeclarationParser
{
    private ReadOnlyParameterTable ParseParameters(MethodDeclarationSyntax methodDeclaration, ReadOnlyFieldTable fieldTable, Unsupported unsupported)
    {
        ParameterTable ParameterTable = new();

        foreach (ParameterSyntax Parameter in methodDeclaration.ParameterList.Parameters)
        {
            ParameterName ParameterName = new() { Text = Parameter.Identifier.ValueText };

            // Ignore duplicate names, the compiler will catch them.
            if (!ParameterTable.ContainsItem(ParameterName))
            {
                if (IsParameterSupported(Parameter, fieldTable, out ExpressionType ParameterType))
                {
                    Parameter NewParameter = new Parameter() { Name = ParameterName, Type = ParameterType };
                    ParameterTable.AddItem(NewParameter);
                }
                else
                {
                    Location Location = Parameter.GetLocation();
                    unsupported.AddUnsupportedParameter(Location);
                }
            }
        }

        return ParameterTable.ToReadOnly();
    }

    private bool IsParameterSupported(ParameterSyntax parameter, ReadOnlyFieldTable fieldTable, out ExpressionType parameterType)
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

        if (!IsTypeSupported(parameter.Type, out parameterType))
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

        if (TryFindFieldByName(fieldTable, ParameterName, out _))
        {
            LogWarning($"Parameter '{ParameterName}' is already the name of a field.");

            IsParameterSupported = false;
        }

        return IsParameterSupported;
    }

    private bool TryFindParameterByName(ReadOnlyParameterTable parameterTable, string parameterName, out IParameter parameter)
    {
        foreach (KeyValuePair<ParameterName, Parameter> Entry in parameterTable)
            if (Entry.Value.Name.Text == parameterName)
            {
                parameter = Entry.Value;
                return true;
            }

        parameter = null!;
        return false;
    }
}
