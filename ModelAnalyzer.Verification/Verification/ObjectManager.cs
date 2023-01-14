namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Z3;

/// <summary>
/// Represents an object manager.
/// </summary>
internal class ObjectManager
{
    /// <summary>
    /// Gets the name of the root object.
    /// </summary>
    public const string ThisObject = "this";

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectManager"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ObjectManager(SolverContext context)
    {
        Context = context;
        ObjectIndex = 1;
    }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    public SolverContext Context { get; }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    required public Solver Solver { get; init; }

    /// <summary>
    /// Gets the class models.
    /// </summary>
    required public Dictionary<string, ClassModel> ClassModelTable { get; init; }

    /// <summary>
    /// Gets or sets the table of aliases.
    /// </summary>
    public AliasTable AliasTable { get; set; } = new();

    public Expr CreateVariable(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        Expr? InitializerExpr;

        if (variableInitializer is not null || initWithDefault)
            InitializerExpr = CreateInitializerExpr(variableType, variableInitializer);
        else
            InitializerExpr = null;

        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch: null, InitializerExpr);
    }

    public Expr CreateVariable(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, BoolExpr? branch, Expr? initializerExpr)
    {
        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch, initializerExpr);
    }

    private Expr CreateVariableInternal(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, BoolExpr? branch, Expr? initializerExpr)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);

        AliasTable.AddOrIncrement(Variable);

        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);
        Expr VariableExpr = CreateVariableExpr(VariableNameAlias, variableType);

        if (initializerExpr is not null)
        {
            BoolExpr InitExpr = Context.CreateEqualExpr(VariableExpr, initializerExpr);
            Context.AddToSolver(Solver, branch, InitExpr);
        }

        return VariableExpr;
    }

    public Expr CreateValueExpr(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
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

        Context.AddToSolver(Solver, branch, Context.CreateEqualExpr(DestinationExpr, sourceExpr));
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
            BoolExpr WhenTrueInitExpr = Context.CreateEqualExpr(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = whenFalseAliasTable.GetAlias(Variable);
            Expr WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias, VariableType);
            BoolExpr WhenFalseInitExpr = Context.CreateEqualExpr(DestinationExpr, WhenFalseSourceExpr);

            Context.AddToSolver(Solver, branchTrue, WhenTrueInitExpr);
            Context.AddToSolver(Solver, branchFalse, WhenFalseInitExpr);
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
            BoolExpr InitExpr = Context.CreateEqualExpr(VariableExpr, InitializerExpr);

            Context.AddToSolver(Solver, branch, InitExpr);
        }
    }

    private Expr CreateVariableExpr(VariableAlias alias, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        string AliasString = alias.ToString();
        Expr Result;

        Dictionary<ExpressionType, Func<string, Expr>> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.CreateBooleanConstant },
            { ExpressionType.Integer, Context.CreateIntegerConstant },
            { ExpressionType.FloatingPoint, Context.CreateFloatingPointConstant },
        };

        if (variableType.IsSimple)
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](AliasString);
        }
        else
            Result = Context.CreateReferenceConstant(AliasString);

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

        if (variableInitializer is NewObjectExpression NewObject)
            Result = CreateObjectInitializer(NewObject);
        else if (variableInitializer is LiteralNullExpression LiteralNull)
            return Context.Null;
        else
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](variableInitializer);
        }

        return Result;
    }

    private Expr CreateBooleanInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralBooleanValueExpression LiteralBoolean)
            return Context.CreateBooleanValue(LiteralBoolean.Value);
        else
            return Context.False;
    }

    private Expr CreateIntegerInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralIntegerValueExpression LiteralInteger)
            return Context.CreateIntegerValue(LiteralInteger.Value);
        else
            return Context.Zero;
    }

    private Expr CreateFloatingPointInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
            return Context.CreateFloatingPointValue(LiteralFloatingPoint.Value);
        else
            return Context.Zero;
    }

    public Expr CreateObjectInitializer(NewObjectExpression newObjectExpression)
    {
        string ClassName = newObjectExpression.ObjectType.Name;

        Debug.Assert(ClassModelTable.ContainsKey(ClassName));

        ClassModel TypeClassModel = ClassModelTable[ClassName];

        Expr Result = Context.CreateReferenceValue(ObjectIndex);

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            // TODO: properties
        }

        ObjectIndex++;

        return Result;
    }

    public Local FindOrCreateResultLocal(Method hostMethod, ExpressionType returnType)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in hostMethod.LocalTable)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        LocalName ResultLocalName = new LocalName() { Text = Ensure.ResultKeyword };
        Local ResultLocal = new Local() { Name = ResultLocalName, Type = returnType, Initializer = null };
        LocalName ResultBlockLocalName = CreateBlockName(owner: null, hostMethod, ResultLocal.Name);
        Variable ResultLocalVariable = new(ResultBlockLocalName, returnType);

        AliasTable.AddOrIncrement(ResultLocalVariable);

        return ResultLocal;
    }

    public static LocalName CreateBlockName(string? owner, Method? hostMethod, IVariableName localName)
    {
        LocalName Result = null!;

        if (hostMethod is not null)
        {
            string LocalBlockText = $"{hostMethod.Name.Text}-${localName.Text}";
            Result = new LocalName() { Text = LocalBlockText };
        }

        if (owner is not null)
        {
            string OwnerText = $"{owner}:${localName.Text}";
            Result = new LocalName() { Text = OwnerText };
        }

        Debug.Assert(Result is not null);

        return Result!;
    }

    public Expr GetDefaultExpr(ExpressionType variableType)
    {
        Dictionary<ExpressionType, Expr> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.False },
            { ExpressionType.Integer, Context.Zero },
            { ExpressionType.FloatingPoint, Context.Zero },
        };

        Expr Result;

        if (variableType.IsSimple)
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType];
        }
        else
            Result = Context.Null;

        return Result;
    }

    private int ObjectIndex;
}
