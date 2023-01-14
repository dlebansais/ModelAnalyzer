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

    public IExprSet<IExprCapsule> CreateVariable(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        IExprSet<IExprCapsule>? InitializerExpr;

        if (variableInitializer is not null || initWithDefault)
            InitializerExpr = CreateInitializerExpr(variableType, variableInitializer);
        else
            InitializerExpr = null;

        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch: null, InitializerExpr);
    }

    public IExprSet<IExprCapsule> CreateVariable(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, IBoolExprCapsule? branch, IExprSet<IExprCapsule>? initializerExpr)
    {
        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch, initializerExpr);
    }

    private IExprSet<IExprCapsule> CreateVariableInternal(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, IBoolExprCapsule? branch, IExprSet<IExprCapsule>? initializerExpr)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);

        AliasTable.AddOrIncrement(Variable);

        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);
        IExprSet<IExprCapsule> VariableExpr = CreateVariableExpr(VariableNameAlias, variableType);

        if (initializerExpr is not null)
        {
            IExprSet<IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, initializerExpr);
            Context.AddToSolver(Solver, branch, InitExpr);
        }

        return VariableExpr;
    }

    public IExprSet<IExprCapsule> CreateValueExpr(string? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);
        VariableAlias VariableAlias = AliasTable.GetAlias(Variable);
        IExprSet<IExprCapsule> ResultExprSet = CreateVariableExpr(VariableAlias, variableType);

        return ResultExprSet;
    }

    public void Assign(IBoolExprCapsule? branch, Variable destination, IExprSet<IExprCapsule> sourceExpr)
    {
        AliasTable.IncrementAlias(destination);

        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        IExprSet<IExprCapsule> DestinationExpr = CreateVariableExpr(DestinationNameAlias, destination.Type);

        Context.AddToSolver(Solver, branch, Context.CreateEqualExprSet(DestinationExpr, sourceExpr));
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

    public void MergeBranches(AliasTable whenTrueAliasTable, IBoolExprCapsule branchTrue, List<VariableAlias> aliasesOnlyWhenTrue, AliasTable whenFalseAliasTable, IBoolExprCapsule branchFalse, List<VariableAlias> aliasesOnlyWhenFalse)
    {
        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        AliasTable.Merge(whenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(branchTrue, aliasesOnlyWhenFalse);
        AddConditionalAliases(branchFalse, aliasesOnlyWhenTrue);

        foreach (Variable Variable in UpdatedNameList)
        {
            ExpressionType VariableType = Variable.Type;

            VariableAlias NameAlias = AliasTable.GetAlias(Variable);
            IExprSet<IExprCapsule> DestinationExpr = CreateVariableExpr(NameAlias, VariableType);

            VariableAlias WhenTrueNameAlias = whenTrueAliasTable.GetAlias(Variable);
            IExprSet<IExprCapsule> WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias, VariableType);
            IExprSet<IBoolExprCapsule> WhenTrueInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = whenFalseAliasTable.GetAlias(Variable);
            IExprSet<IExprCapsule> WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias, VariableType);
            IExprSet<IBoolExprCapsule> WhenFalseInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenFalseSourceExpr);

            Context.AddToSolver(Solver, branchTrue, WhenTrueInitExpr);
            Context.AddToSolver(Solver, branchFalse, WhenFalseInitExpr);
        }
    }

    private void AddConditionalAliases(IBoolExprCapsule branch, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            Variable Variable = Alias.Variable;
            ExpressionType VariableType = Variable.Type;

            IExprSet<IExprCapsule> VariableExpr = CreateVariableExpr(Alias, VariableType);
            IExprSet<IExprCapsule> InitializerExpr = GetDefaultExpr(VariableType);
            IExprSet<IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, InitializerExpr);

            Context.AddToSolver(Solver, branch, InitExpr);
        }
    }

    private IExprSet<IExprCapsule> CreateVariableExpr(VariableAlias alias, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        string AliasString = alias.ToString();
        IExprSet<IExprCapsule> Result;

        Dictionary<ExpressionType, Func<string, IExprSet<IExprCapsule>>> SwitchTable = new()
        {
            { ExpressionType.Boolean, CreateBooleanConstant },
            { ExpressionType.Integer, CreateIntegerConstant },
            { ExpressionType.FloatingPoint, CreateFloatingPointConstant },
        };

        if (variableType.IsSimple)
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](AliasString);
        }
        else
            Result = CreateReferenceConstant(alias, variableType);

        return Result;
    }

    private IExprSet<IExprCapsule> CreateInitializerExpr(ExpressionType variableType, ILiteralExpression? variableInitializer)
    {
        Dictionary<ExpressionType, Func<ILiteralExpression?, IExprSet<IExprCapsule>>> SwitchTable = new()
        {
            { ExpressionType.Boolean, CreateBooleanInitializer },
            { ExpressionType.Integer, CreateIntegerInitializer },
            { ExpressionType.FloatingPoint, CreateFloatingPointInitializer },
        };

        IExprSet<IExprCapsule> Result;

        if (variableInitializer is NewObjectExpression NewObject)
            Result = CreateObjectInitializer(NewObject.ObjectType);
        else if (variableInitializer is LiteralNullExpression LiteralNull)
            Result = CreateNullInitializer(variableType);
        else
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](variableInitializer);
        }

        return Result;
    }

    private IExprSet<IBoolExprCapsule> CreateBooleanInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralBooleanValueExpression LiteralBoolean)
        {
            IBoolExprCapsule Expr = Context.CreateBooleanValue(LiteralBoolean.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.FalseSet;
    }

    private IExprSet<IIntExprCapsule> CreateIntegerInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralIntegerValueExpression LiteralInteger)
        {
            IIntExprCapsule Expr = Context.CreateIntegerValue(LiteralInteger.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    private IExprSet<IArithExprCapsule> CreateFloatingPointInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
        {
            IArithExprCapsule Expr = Context.CreateFloatingPointValue(LiteralFloatingPoint.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    public IExprSet<IExprCapsule> CreateObjectInitializer(ExpressionType expressionType)
    {
        ClassModel TypeClassModel = TypeToModel(expressionType);
        IExprCapsule ReferenceResult = Context.CreateReferenceValue(ObjectIndex++);

        List<IExprSet<IExprCapsule>> PropertySetList = new();
        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;
            IExprSet<IExprCapsule> PropertyExpressions = CreateInitializerExpr(Property.Type, Property.Initializer);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprSet<IExprCapsule> Result = new(ReferenceResult, PropertySetList);

        return Result;
    }

    public IExprSet<IExprCapsule> CreateNullInitializer(ExpressionType expressionType)
    {
        ClassModel TypeClassModel = TypeToModel(expressionType);
        List<IExprSet<IExprCapsule>> PropertySetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;
            IExprSet<IExprCapsule> PropertyExpressions = CreateInitializerExpr(Property.Type, variableInitializer: null);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprSet<IExprCapsule> Result = new(Context.Null, PropertySetList);

        return Result;
    }

    private IExprSet<IBoolExprCapsule> CreateBooleanConstant(string alias)
    {
        return Context.CreateBooleanConstant(alias).ToSingleSet();
    }

    private IExprSet<IIntExprCapsule> CreateIntegerConstant(string alias)
    {
        return Context.CreateIntegerConstant(alias).ToSingleSet();
    }

    private IExprSet<IArithExprCapsule> CreateFloatingPointConstant(string alias)
    {
        return Context.CreateFloatingPointConstant(alias).ToSingleSet();
    }

    public IExprSet<IExprCapsule> CreateReferenceConstant(VariableAlias alias, ExpressionType variableType)
    {
        ClassModel TypeClassModel = TypeToModel(variableType);
        IExprCapsule ReferenceResult = Context.CreateReferenceConstant(alias.ToString());
        List<IExprSet<IExprCapsule>> PropertySetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;

            IVariableName PropertyBlockName = CreateBlockName(TypeClassModel.Name, hostMethod: null, Property.Name);
            Variable PropertyVariable = new(PropertyBlockName, Property.Type);

            AliasTable.AddOrIncrement(PropertyVariable);

            VariableAlias PropertyVariableNameAlias = AliasTable.GetAlias(PropertyVariable);

            IExprSet<IExprCapsule> PropertyExpressions = CreateVariableExpr(PropertyVariableNameAlias, Property.Type);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprSet<IExprCapsule> Result = new(ReferenceResult, PropertySetList);

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

    public IExprSet<IExprCapsule> GetDefaultExpr(ExpressionType variableType)
    {
        Dictionary<ExpressionType, IExprSet<IExprCapsule>> SwitchTable = new()
        {
            { ExpressionType.Boolean, Context.FalseSet },
            { ExpressionType.Integer, Context.ZeroSet },
            { ExpressionType.FloatingPoint, Context.ZeroSet },
        };

        IExprSet<IExprCapsule> Result;

        if (variableType.IsSimple)
        {
            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType];
        }
        else
            Result = CreateNullDefault(variableType);

        return Result;
    }

    public IExprSet<IExprCapsule> CreateNullDefault(ExpressionType variableType)
    {
        ClassModel TypeClassModel = TypeToModel(variableType);
        List<IExprSet<IExprCapsule>> PropertySetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;

            IExprSet<IExprCapsule> PropertyExpressions = GetDefaultExpr(Property.Type);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprSet<IExprCapsule> Result = new(Context.Null, PropertySetList);

        return Result;
    }

    public ClassModel TypeToModel(ExpressionType type)
    {
        Debug.Assert(!type.IsSimple);

        string ClassName = type.Name;

        Debug.Assert(ClassModelTable.ContainsKey(ClassName));

        return ClassModelTable[ClassName];
    }

    private int ObjectIndex;
}
