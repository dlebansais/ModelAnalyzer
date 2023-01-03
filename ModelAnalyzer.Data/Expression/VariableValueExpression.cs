namespace ModelAnalyzer;

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

/// <summary>
/// Represents a variable as an expression.
/// </summary>
internal class VariableValueExpression : Expression
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override bool IsSimple => true;

    /// <inheritdoc/>
    public override ExpressionType GetExpressionType(IMemberCollectionContext memberCollectionContext)
    {
        IVariable? Variable = null;

        foreach (Property Property in memberCollectionContext.GetProperties())
            if (Property.Name.Text == VariableName.Text)
            {
                Variable = Property;
                break;
            }

        foreach (Field Field in memberCollectionContext.GetFields())
            if (Field.Name.Text == VariableName.Text)
            {
                Variable = Field;
                break;
            }

        if (memberCollectionContext.HostMethod is Method HostMethod)
        {
            foreach (KeyValuePair<ParameterName, Parameter> Entry in HostMethod.ParameterTable)
                if (Entry.Key.Text == VariableName.Text)
                {
                    Debug.Assert(Variable is null);

                    Variable = Entry.Value;
                    break;
                }

            foreach (KeyValuePair<LocalName, Local> Entry in HostMethod.LocalTable)
                if (Entry.Key.Text == VariableName.Text)
                {
                    Debug.Assert(Variable is null);

                    Variable = Entry.Value;
                    break;
                }

            if (Variable is null && memberCollectionContext.ResultLocal is Local ResultLocal && VariableName.Text == Ensure.ResultKeyword)
                Variable = ResultLocal;
        }

        Debug.Assert(Variable is not null);

        return Variable!.Type;
    }

    /// <summary>
    /// Gets the variable.
    /// </summary>
    required public IVariableName VariableName { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return VariableName.Text;
    }
}
