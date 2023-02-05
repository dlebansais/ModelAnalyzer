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
    /// Initializes a new instance of the <see cref="ObjectManager"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ObjectManager(SolverContext context)
    {
        Context = context;
        RootInstanceExpr = Context.CreateObjectReferenceValue(ClassName.FromSimpleString("this"), ReferenceIndex.Root);
        Index = ReferenceIndex.First;
    }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    public SolverContext Context { get; }

    /// <summary>
    /// Gets the expressions for the root instance.
    /// </summary>
    public IObjectRefExprCapsule RootInstanceExpr { get; }

    /// <summary>
    /// Gets the solver.
    /// </summary>
    required public Solver Solver { get; init; }

    /// <summary>
    /// Gets the class models.
    /// </summary>
    required public ClassModelTable ClassModelTable { get; init; }

    /// <summary>
    /// Gets or sets the table of aliases.
    /// </summary>
    public AliasTable AliasTable { get; set; } = new();

    public IExprSet<IExprCapsule, IExprCapsule> CreateVariable(Instance? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        IExprSet<IExprCapsule, IExprCapsule>? InitializerExpr;

        if (variableInitializer is not null || initWithDefault)
            InitializerExpr = CreateInitializerExpr(variableType, variableInitializer);
        else
            InitializerExpr = null;

        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch: null, InitializerExpr);
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateVariable(Instance? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, IBoolExprCapsule? branch, IExprSet<IExprCapsule, IExprCapsule>? initializerExpr)
    {
        return CreateVariableInternal(owner, hostMethod, variableName, variableType, branch, initializerExpr);
    }

    private IExprSet<IExprCapsule, IExprCapsule> CreateVariableInternal(Instance? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType, IBoolExprCapsule? branch, IExprSet<IExprCapsule, IExprCapsule>? initializerExpr)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);

        AliasTable.AddOrIncrement(Variable);

        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);
        IExprSet<IExprCapsule, IExprCapsule> VariableExpr = CreateVariableExpr(VariableNameAlias, variableType);

        if (initializerExpr is not null)
        {
            IExprSet<IBoolExprCapsule, IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, initializerExpr);
            Context.AddToSolver(Solver, branch, InitExpr);
        }

        return VariableExpr;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateValueExpr(Instance? owner, Method? hostMethod, IVariableName variableName, ExpressionType variableType)
    {
        IVariableName VariableBlockName = CreateBlockName(owner, hostMethod, variableName);
        Variable Variable = new(VariableBlockName, variableType);
        VariableAlias VariableAlias = AliasTable.GetAlias(Variable);
        IExprSet<IExprCapsule, IExprCapsule> ResultExprSet = CreateVariableExpr(VariableAlias, variableType);

        return ResultExprSet;
    }

    public void Assign(IBoolExprCapsule? branch, Variable destination, IExprSet<IExprCapsule, IExprCapsule> sourceExpr)
    {
        AliasTable.IncrementAlias(destination);

        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        IExprSet<IExprCapsule, IExprCapsule> DestinationExpr = CreateVariableExpr(DestinationNameAlias, destination.Type);

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
            IExprSet<IExprCapsule, IExprCapsule> DestinationExpr = CreateVariableExpr(NameAlias, VariableType);

            VariableAlias WhenTrueNameAlias = whenTrueAliasTable.GetAlias(Variable);
            IExprSet<IExprCapsule, IExprCapsule> WhenTrueSourceExpr = CreateVariableExpr(WhenTrueNameAlias, VariableType);
            IExprSet<IBoolExprCapsule, IBoolExprCapsule> WhenTrueInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = whenFalseAliasTable.GetAlias(Variable);
            IExprSet<IExprCapsule, IExprCapsule> WhenFalseSourceExpr = CreateVariableExpr(WhenFalseNameAlias, VariableType);
            IExprSet<IBoolExprCapsule, IBoolExprCapsule> WhenFalseInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenFalseSourceExpr);

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

            IExprSet<IExprCapsule, IExprCapsule> VariableExpr = CreateVariableExpr(Alias, VariableType);
            IExprSet<IExprCapsule, IExprCapsule> InitializerExpr = GetDefaultExpr(VariableType);
            IExprSet<IBoolExprCapsule, IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, InitializerExpr);

            Context.AddToSolver(Solver, branch, InitExpr);
        }
    }

    private IExprSet<IExprCapsule, IExprCapsule> CreateVariableExpr(VariableAlias alias, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        string AliasString = alias.ToString();
        IExprSet<IExprCapsule, IExprCapsule> Result;

        if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, Func<string, IExprSet<IExprCapsule, IExprCapsule>>> SwitchTable = new()
            {
                { ExpressionType.Boolean, CreateBooleanConstant },
                { ExpressionType.Integer, CreateIntegerConstant },
                { ExpressionType.FloatingPoint, CreateFloatingPointConstant },
            };

            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](AliasString);
        }
        else if (variableType.IsArray)
            Result = CreateArrayReferenceConstant(alias, variableType.ToElementType());
        else
            Result = CreateObjectReferenceConstant(alias, variableType);

        return Result;
    }

    private IExprSet<IExprCapsule, IExprCapsule> CreateInitializerExpr(ExpressionType variableType, ILiteralExpression? variableInitializer)
    {
        IExprSet<IExprCapsule, IExprCapsule> Result;

        if (variableInitializer is NewObjectExpression NewObject)
            Result = CreateObjectInitializer(NewObject.ObjectType);
        else if (variableInitializer is NewArrayExpression NewArray)
            Result = CreateArrayInitializer(NewArray.ArrayType, NewArray.ArraySize);
        else if (variableInitializer is LiteralNullExpression LiteralNull)
            Result = CreateNullInitializer(variableType);
        else if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, Func<ILiteralExpression?, IExprSet<IExprCapsule, IExprCapsule>>> SwitchTable = new()
            {
                { ExpressionType.Boolean, CreateBooleanInitializer },
                { ExpressionType.Integer, CreateIntegerInitializer },
                { ExpressionType.FloatingPoint, CreateFloatingPointInitializer },
            };

            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](variableInitializer);
        }
        else
        {
            Debug.Assert(variableInitializer is null);
            Result = CreateNullInitializer(variableType);
        }

        return Result;
    }

    private IExprSet<IBoolExprCapsule, IBoolExprCapsule> CreateBooleanInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralBooleanValueExpression LiteralBoolean)
        {
            IBoolExprCapsule Expr = Context.CreateBooleanValue(LiteralBoolean.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.FalseSet;
    }

    private IExprSet<IIntExprCapsule, IIntExprCapsule> CreateIntegerInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralIntegerValueExpression LiteralInteger)
        {
            IIntExprCapsule Expr = Context.CreateIntegerValue(LiteralInteger.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    private IExprSet<IArithExprCapsule, IArithExprCapsule> CreateFloatingPointInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
        {
            IArithExprCapsule Expr = Context.CreateFloatingPointValue(LiteralFloatingPoint.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateObjectInitializer(ExpressionType expressionType)
    {
        ClassModel TypeClassModel = TypeToModel(expressionType);
        IObjectRefExprCapsule ReferenceResult = Context.CreateObjectReferenceValue(TypeClassModel.ClassName, Index.Increment());

        return CreateObjectInitializer(expressionType, ReferenceResult);
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateObjectInitializer(ExpressionType expressionType, IRefExprCapsule referenceResult)
    {
        Debug.Assert(!expressionType.IsArray);

        List<IExprSet<IExprCapsule, IExprCapsule>> VariableSetList = new();

        ClassModel TypeClassModel = TypeToModel(expressionType);

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;
            IExprSet<IExprCapsule, IExprCapsule> PropertyExpressions = CreateInitializerExpr(Property.Type, Property.Initializer);
            VariableSetList.Add(PropertyExpressions);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in TypeClassModel.FieldTable)
        {
            Field Field = Entry.Value;
            IExprSet<IExprCapsule, IExprCapsule> FieldExpressions = CreateInitializerExpr(Field.Type, Field.Initializer);
            VariableSetList.Add(FieldExpressions);
        }

        ExprObject Result = new(referenceResult, VariableSetList);

        return Result;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateArrayInitializer(ExpressionType expressionType, ArraySize arraySize)
    {
        Debug.Assert(expressionType.IsArray);
        Debug.Assert(arraySize.IsKnown);

        ExpressionType ElementType = expressionType.ToElementType();
        IArrayRefExprCapsule ReferenceResult = Context.CreateArrayReferenceValue(ElementType, Index.Increment());

        return CreateArrayInitializer(ElementType, arraySize, ReferenceResult);
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateArrayInitializer(ExpressionType elementType, ArraySize arraySize, IRefExprCapsule referenceResult)
    {
        ExprArray<IExprCapsule> Result = new(referenceResult, new List<IExprCapsule>());

        return Result;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateNullInitializer(ExpressionType expressionType)
    {
        if (expressionType.IsArray)
            return CreateArrayInitializer(expressionType, ArraySize.Unknown, Context.Null);
        else
            return CreateObjectInitializer(expressionType, Context.Null);
    }

    private IExprSet<IBoolExprCapsule, IBoolExprCapsule> CreateBooleanConstant(string alias)
    {
        return Context.CreateBooleanConstant(alias).ToSingleSet();
    }

    private IExprSet<IIntExprCapsule, IIntExprCapsule> CreateIntegerConstant(string alias)
    {
        return Context.CreateIntegerConstant(alias).ToSingleSet();
    }

    private IExprSet<IArithExprCapsule, IArithExprCapsule> CreateFloatingPointConstant(string alias)
    {
        return Context.CreateFloatingPointConstant(alias).ToSingleSet();
    }

    private IExprSet<IExprCapsule, IExprCapsule> CreateArrayReferenceConstant(VariableAlias alias, ExpressionType elementType)
    {
        IArrayRefExprCapsule ReferenceResult = Context.CreateArrayReferenceConstant(elementType, alias.ToString());

        IVariableName ElementBlockName = CreateBlockElementName(ReferenceResult);
        Variable ElementVariable = new(ElementBlockName, elementType);

        if (!AliasTable.ContainsVariable(ElementVariable))
            AliasTable.AddVariable(ElementVariable);

        VariableAlias ElementVariableNameAlias = AliasTable.GetAlias(ElementVariable);

        ExprArray<IExprCapsule> Result = new(ReferenceResult, new List<IExprCapsule>());

        return Result;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateObjectReferenceConstant(VariableAlias alias, ExpressionType variableType)
    {
        ClassModel ReferenceClassModel = TypeToModel(variableType);
        IObjectRefExprCapsule ReferenceResult = Context.CreateObjectReferenceConstant(ReferenceClassModel.ClassName, alias.ToString());
        Instance Reference = new() { ClassModel = ReferenceClassModel, Expr = ReferenceResult };
        List<IExprSet<IExprCapsule, IExprCapsule>> VariableSetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in ReferenceClassModel.PropertyTable)
        {
            Property Property = Entry.Value;

            IVariableName PropertyBlockName = CreateBlockName(Reference, hostMethod: null, Property.Name);
            Variable PropertyVariable = new(PropertyBlockName, Property.Type);

            if (!AliasTable.ContainsVariable(PropertyVariable))
                AliasTable.AddVariable(PropertyVariable);

            VariableAlias PropertyVariableNameAlias = AliasTable.GetAlias(PropertyVariable);

            IExprSet<IExprCapsule, IExprCapsule> PropertyExpressions = CreateVariableExpr(PropertyVariableNameAlias, Property.Type);
            VariableSetList.Add(PropertyExpressions);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in ReferenceClassModel.FieldTable)
        {
            Field Field = Entry.Value;

            IVariableName FieldBlockName = CreateBlockName(Reference, hostMethod: null, Field.Name);
            Variable FieldVariable = new(FieldBlockName, Field.Type);

            if (!AliasTable.ContainsVariable(FieldVariable))
                AliasTable.AddVariable(FieldVariable);

            VariableAlias FieldVariableNameAlias = AliasTable.GetAlias(FieldVariable);

            IExprSet<IExprCapsule, IExprCapsule> FieldExpressions = CreateVariableExpr(FieldVariableNameAlias, Field.Type);
            VariableSetList.Add(FieldExpressions);
        }

        ExprObject Result = new(ReferenceResult, VariableSetList);

        return Result;
    }

    public Local FindOrCreateResultLocal(Method hostMethod, ExpressionType returnType)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in hostMethod.LocalTable)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        Local? ResultLocal = hostMethod.ResultLocal as Local;
        Debug.Assert(ResultLocal is not null);

        LocalName ResultBlockLocalName = CreateBlockName(owner: null, hostMethod, ResultLocal!.Name);
        Variable ResultLocalVariable = new(ResultBlockLocalName, returnType);

        AliasTable.AddOrIncrement(ResultLocalVariable);

        return ResultLocal;
    }

    public static LocalName CreateBlockLocalName(Method hostMethod, IVariableName localName)
    {
        string LocalBlockText = $"{hostMethod.ClassName}::{hostMethod.Name.Text}-${localName.Text}";
        return new LocalName() { Text = LocalBlockText };
    }

    public static LocalName CreateBlockOwnerName(Instance owner, IVariableName localName)
    {
        Debug.Assert(owner.Expr is IObjectRefExprCapsule);

        IObjectRefExprCapsule ObjectExpr = (IObjectRefExprCapsule)owner.Expr;
        ClassName ClassName = ObjectExpr.ClassName;

        Debug.Assert(ClassName != ClassName.Empty);

        string OwnerText = $"{ClassName}#{ObjectExpr.Index}:${localName.Text}";

        return new LocalName() { Text = OwnerText };
    }

    public static LocalName CreateBlockElementName(IArrayRefExprCapsule expr)
    {
        string OwnerText = $"[]{expr.Index}";

        return new LocalName() { Text = OwnerText };
    }

    public static LocalName CreateBlockName(Instance? owner, Method? hostMethod, IVariableName localName)
    {
        LocalName Result = null!;

        if (hostMethod is not null)
        {
            Result = CreateBlockLocalName(hostMethod, localName);
        }

        if (owner is not null)
        {
            Result = CreateBlockOwnerName(owner, localName);
        }

        Debug.Assert(Result is not null);

        return Result!;
    }

    public IExprSet<IExprCapsule, IExprCapsule> GetDefaultExpr(ExpressionType variableType)
    {
        IExprSet<IExprCapsule, IExprCapsule> Result;

        if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, IExprSet<IExprCapsule, IExprCapsule>> SwitchTable = new()
            {
                { ExpressionType.Boolean, Context.FalseSet },
                { ExpressionType.Integer, Context.ZeroSet },
                { ExpressionType.FloatingPoint, Context.ZeroSet },
            };

            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType];
        }
        else if (variableType.IsArray)
            Result = CreateArrayNullDefault(variableType);
        else
            Result = CreateObjectNullDefault(variableType);

        return Result;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateObjectNullDefault(ExpressionType variableType)
    {
        ClassModel TypeClassModel = TypeToModel(variableType);
        List<IExprSet<IExprCapsule, IExprCapsule>> PropertySetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;

            IExprSet<IExprCapsule, IExprCapsule> PropertyExpressions = GetDefaultExpr(Property.Type);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprObject Result = new(Context.Null, PropertySetList);

        return Result;
    }

    public IExprSet<IExprCapsule, IExprCapsule> CreateArrayNullDefault(ExpressionType variableType)
    {
        Debug.Assert(variableType.IsArray);

        ExprArray<IExprCapsule> Result = new(Context.Null, new List<IExprCapsule>());

        return Result;
    }

    public void ClearState(Instance reference)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in reference.ClassModel.PropertyTable)
        {
            LocalName PropertyBlockLocalName = CreateBlockName(reference, hostMethod: null, Entry.Key);
            Variable PropertyVariable = new(PropertyBlockLocalName, Entry.Value.Type);

            AliasTable.IncrementAlias(PropertyVariable);
        }
    }

    public ClassModel TypeToModel(ExpressionType type)
    {
        Debug.Assert(!type.IsSimple);

        ClassName ClassName = type.TypeName;

        Debug.Assert(ClassModelTable.ContainsKey(ClassName));

        return ClassModelTable[ClassName];
    }

    private ReferenceIndex Index;
}
