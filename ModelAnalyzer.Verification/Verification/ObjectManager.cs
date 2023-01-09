namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using AnalysisLogger;
using Microsoft.Z3;

/// <summary>
/// Represents an object manager.
/// </summary>
internal class ObjectManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectManager"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ObjectManager(Context context)
    {
        Context = context;

        // Need model generation turned on.
        Zero = Context.MkInt(0);
        False = Context.MkBool(false);
        True = Context.MkBool(true);
    }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    public Context Context { get; }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    required public Solver Solver { get; init; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    required public IAnalysisLogger Logger { get; init; }

    /// <summary>
    /// Gets the class models.
    /// </summary>
    required public Dictionary<string, ClassModel> ClassModelTable { get; init; }

    /// <summary>
    /// Gets or sets the table of aliases.
    /// </summary>
    public AliasTable AliasTable { get; set; } = new();

    public Expr CreateVariable(Method? hostMethod, IVariableName variableName, ExpressionType variableType, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        Expr? InitializerExpr;

        if (variableInitializer is not null || initWithDefault)
            InitializerExpr = CreateInitializerExpr(variableType, variableInitializer);
        else
            InitializerExpr = null;

        return CreateVariableInternal(hostMethod, variableName, variableType, branch: null, InitializerExpr);
    }

    public Expr CreateVariable(Method? hostMethod, IVariableName variableName, ExpressionType variableType, BoolExpr? branch, Expr? initializerExpr)
    {
        return CreateVariableInternal(hostMethod, variableName, variableType, branch, initializerExpr);
    }

    private Expr CreateVariableInternal(Method? hostMethod, IVariableName variableName, ExpressionType variableType, BoolExpr? branch, Expr? initializerExpr)
    {
        IVariableName VariableBlockName = CreateBlockName(hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);

        AliasTable.AddOrIncrement(Variable);

        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);
        Expr VariableExpr = CreateVariableExpr(VariableNameAlias, variableType);

        if (initializerExpr is not null)
        {
            BoolExpr InitExpr = Context.MkEq(VariableExpr, initializerExpr);
            AddToSolver(branch, InitExpr);
        }

        return VariableExpr;
    }

    public Expr CreateValueExpr(Method? hostMethod, IVariableName variableName, ExpressionType variableType)
    {
        IVariableName VariableBlockName = CreateBlockName(hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);
        VariableAlias VariableAlias = AliasTable.GetAlias(Variable);
        Expr ResultExpr = CreateVariableExpr(VariableAlias, variableType);

        return ResultExpr;
    }

    public void Assign(BoolExpr? branch, Variable destination, Expr sourceExpr)
    {
        AliasTable.IncrementAlias(destination);

        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        Expr DestinationExpr = CreateVariableExpr(DestinationNameAlias, destination.Type);

        AddToSolver(branch, Context.MkEq(DestinationExpr, sourceExpr));
    }

    public void BeginBranch(out AliasTable beginAliasTable)
    {
        beginAliasTable = AliasTable.Clone();
    }

    public void EndBranch(AliasTable beginAliasTable, out List<VariableAlias> aliasesOnlyInBranch, out AliasTable endAliasTable)
    {
        aliasesOnlyInBranch = AliasTable.GetAliasDifference(beginAliasTable);
        endAliasTable = AliasTable.Clone();
    }

    public void MergeBranches(AliasTable whenTrueAliasTable, BoolExpr branchTrue, List<VariableAlias> aliasesOnlyWhenTrue, AliasTable whenFalseAliasTable, BoolExpr branchFalse, List<VariableAlias> aliasesOnlyWhenFalse)
    {
        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        AliasTable.Merge(whenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(branchTrue, aliasesOnlyWhenFalse);
        AddConditionalAliases(branchFalse, aliasesOnlyWhenTrue);

        foreach (Variable Variable in UpdatedNameList)
        {
            ExpressionType VariableType = Variable.Type;

            VariableAlias NameAlias = AliasTable.GetAlias(Variable);
            Expr DestinationExpr = CreateVariableExpr(NameAlias, VariableType);

            VariableAlias WhenTrueNameAlias = whenTrueAliasTable.GetAlias(Variable);
            Expr WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias, VariableType);
            BoolExpr WhenTrueInitExpr = Context.MkEq(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = whenFalseAliasTable.GetAlias(Variable);
            Expr WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias, VariableType);
            BoolExpr WhenFalseInitExpr = Context.MkEq(DestinationExpr, WhenFalseSourceExpr);

            AddToSolver(branchTrue, WhenTrueInitExpr);
            AddToSolver(branchFalse, WhenFalseInitExpr);
        }
    }

    private void AddConditionalAliases(BoolExpr branch, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            Variable Variable = Alias.Variable;
            ExpressionType VariableType = Variable.Type;

            Expr VariableExpr = CreateVariableExpr(Alias, VariableType);
            Expr InitializerExpr = GetDefaultExpr(VariableType);
            BoolExpr InitExpr = Context.MkEq(VariableExpr, InitializerExpr);

            AddToSolver(branch, InitExpr);
        }
    }

    private Expr CreateVariableExpr(VariableAlias alias, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        string AliasString = alias.ToString();
        Expr Result;

        Dictionary<ExpressionType, Func<string, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.MkBoolConst },
            { ExpressionType.Integer, Context.MkIntConst },
            { ExpressionType.FloatingPoint, Context.MkRealConst },
        };

        if (SwitchTable.ContainsKey(variableType))
            Result = SwitchTable[variableType](AliasString);
        else
        {
            string ClassName = variableType.Name;

            Debug.Assert(ClassModelTable.ContainsKey(ClassName));

            ClassModel TypeClassModel = ClassModelTable[ClassName];
            Result = Context.MkIntConst(AliasString);

            foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
            {
                // TODO: properties
            }
        }

        return Result;
    }

    private Expr CreateInitializerExpr(ExpressionType variableType, ILiteralExpression? variableInitializer)
    {
        Dictionary<ExpressionType, Func<ILiteralExpression?, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, CreateBooleanInitializer },
            { ExpressionType.Integer, CreateIntegerInitializer },
            { ExpressionType.FloatingPoint, CreateFloatingPointInitializer },
        };

        Expr Result;

        if (SwitchTable.ContainsKey(variableType))
            Result = SwitchTable[variableType](variableInitializer);
        else if (variableInitializer is LiteralNullExpression LiteralNull)
            return Zero; // TODO
        else
            Result = CreateObjectInitializer(variableType);

        return Result;
    }

    private Expr CreateBooleanInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralBooleanValueExpression LiteralBoolean)
            return LiteralBoolean.Value == true ? True : False;
        else
            return False;
    }

    private Expr CreateIntegerInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralIntegerValueExpression LiteralInteger)
            return LiteralInteger.Value == 0 ? Zero : CreateIntegerExpr(LiteralInteger.Value);
        else
            return Zero;
    }

    private Expr CreateFloatingPointInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
            return LiteralFloatingPoint.Value == 0 ? Zero : CreateFloatingPointExpr(LiteralFloatingPoint.Value);
        else
            return Zero;
    }

    private Expr CreateObjectInitializer(ExpressionType expressionType)
    {
        // TODO
        return Zero;
    }

    private BoolExpr CreateBooleanExpr(bool value)
    {
        return Context.MkBool(value);
    }

    private IntExpr CreateIntegerExpr(int value)
    {
        return Context.MkInt(value);
    }

    private ArithExpr CreateFloatingPointExpr(double value)
    {
        return (ArithExpr)Context.MkNumeral(value.ToString(CultureInfo.InvariantCulture), Context.MkRealSort());
    }

    public Local FindOrCreateResultLocal(Method hostMethod, ExpressionType returnType)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in hostMethod.LocalTable)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        LocalName ResultLocalName = new LocalName() { Text = Ensure.ResultKeyword };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = returnType, Initializer = null };
        IVariableName ResultLocalBlockName = CreateBlockName(hostMethod, ResultLocal.Name);
        Variable ResultLocalVariable = new(ResultLocalBlockName, returnType);

        AliasTable.AddOrIncrement(ResultLocalVariable);

        return ResultLocal;
    }

    private static IVariableName CreateBlockName(Method? hostMethod, IVariableName localName)
    {
        if (hostMethod is not null)
        {
            string LocalBlockText = $"{hostMethod.Name.Text}${localName.Text}";
            return new LocalName() { Text = LocalBlockText };
        }
        else
            return localName;
    }

    private Expr GetDefaultExpr(ExpressionType variableType)
    {
        Dictionary<ExpressionType, Expr> SwitchTable = new()
        {
            { ExpressionType.Boolean, False },
            { ExpressionType.Integer, Zero },
            { ExpressionType.FloatingPoint, Zero },
        };

        Expr Result;

        if (SwitchTable.ContainsKey(variableType))
            Result = SwitchTable[variableType];
        else
            Result = Zero; // TODO

        return Result;
    }

    private void AddToSolver(BoolExpr? branch, BoolExpr boolExpr)
    {
        if (branch is not null)
        {
            Solver.Assert(Context.MkImplies(branch, boolExpr));
            Log($"Adding {branch} => {boolExpr}");
        }
        else
        {
            Solver.Assert(boolExpr);
            Log($"Adding {boolExpr}");
        }
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private IntExpr Zero;
    private BoolExpr False;
    private BoolExpr True;
}
