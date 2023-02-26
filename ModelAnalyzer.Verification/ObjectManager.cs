namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    /// <summary>
    /// Gets the table of array getters.
    /// </summary>
    public Dictionary<VariableAlias, Method> ArrayGetterTable { get; } = new();

    public IExprBase<IExprCapsule, IExprCapsule> CreateVariable(Instance? owner, Method? hostMethod, IVariable variable, ILiteralExpression? variableInitializer, bool initWithDefault)
    {
        IExprBase<IExprCapsule, IExprCapsule>? InitializerExpr;

        if (variableInitializer is not null || initWithDefault)
            InitializerExpr = CreateInitializerExpr(variable.Type, variableInitializer);
        else
            InitializerExpr = null;

        return CreateVariableInternal(owner, hostMethod, variable, branch: null, InitializerExpr);
    }

    public IExprBase<IExprCapsule, IExprCapsule> CreateVariable(Instance? owner, Method? hostMethod, IVariable variable, IBoolExprCapsule? branch, IExprBase<IExprCapsule, IExprCapsule>? initializerExpr)
    {
        return CreateVariableInternal(owner, hostMethod, variable, branch, initializerExpr);
    }

    public static Variable CreateAliasTrackedVariable(Instance? owner, Method? hostMethod, IVariable variable, ExpressionType type)
    {
        string Text = GetBlockNameText(owner, hostMethod, variable);
        Variable Result;

        switch (variable)
        {
            case Field AsField:
                Result = new Field(new FieldName() { Text = Text }, type) { ClassName = AsField.ClassName, Initializer = null };
                break;
            case Property AsProperty:
                Result = new Property(new PropertyName() { Text = Text }, type) { ClassName = AsProperty.ClassName, Initializer = null };
                break;
            case Parameter AsParameter:
                Result = new Parameter(new ParameterName() { Text = Text }, type) { ClassName = AsParameter.ClassName, MethodName = AsParameter.MethodName };
                break;
            case Local AsLocal:
                Result = new Local(new LocalName() { Text = Text }, type) { ClassName = AsLocal.ClassName, MethodName = AsLocal.MethodName, Initializer = null };
                break;
            default:
                Result = new Variable(new VariableName() { Text = Text }, type);
                break;
        }

        // Base new variable => base input variable
        Debug.Assert(Result.GetType().Name != typeof(Variable).Name || variable.GetType().Name == typeof(Variable).Name);

        return Result;
    }

    private IExprBase<IExprCapsule, IExprCapsule> CreateVariableInternal(Instance? owner, Method? hostMethod, IVariable variable, IBoolExprCapsule? branch, IExprBase<IExprCapsule, IExprCapsule>? initializerExpr)
    {
        Variable Variable = CreateAliasTrackedVariable(owner, hostMethod, variable, variable.Type);

        // Increment for parameters of methods called several times.
        AliasTable.AddOrIncrement(Variable);

        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);
        IExprBase<IExprCapsule, IExprCapsule> VariableExpr = CreateVariableExpr(owner, hostMethod, VariableNameAlias, variable.Type);

        if (initializerExpr is not null)
        {
            IExprSet<IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, initializerExpr);
            Context.AddToSolver(Solver, branch, InitExpr);
        }

        return VariableExpr;
    }

    public IExprBase<IExprCapsule, IExprCapsule> CreateValueExpr(Instance? owner, Method? hostMethod, IVariable variable)
    {
        Variable Variable = CreateAliasTrackedVariable(owner, hostMethod, variable, variable.Type);
        VariableAlias VariableAlias = AliasTable.GetAlias(Variable);
        IExprBase<IExprCapsule, IExprCapsule> ResultExprSet = CreateVariableExpr(owner, hostMethod, VariableAlias, variable.Type);

        return ResultExprSet;
    }

    public void Assign(Instance? owner, Method? hostMethod, IBoolExprCapsule? branch, Variable destination, IExprBase<IExprCapsule, IExprCapsule> sourceExpr)
    {
        AliasTable.IncrementAlias(destination);

        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        IExprBase<IExprCapsule, IExprCapsule> DestinationExpr = CreateVariableExpr(owner, hostMethod, DestinationNameAlias, destination.Type);

        Context.AddToSolver(Solver, branch, Context.CreateEqualExprSet(DestinationExpr, sourceExpr));
    }

    public void AssignElement(IBoolExprCapsule? branch, Variable destination, IIntExprCapsule destinationIndexExpr, IExprCapsule sourceExpr)
    {
        ExprArray<IArrayExprCapsule> DestinationWithStoredElementExpr = CreateDestinationWithStoredElement(branch, destination, destinationIndexExpr, sourceExpr);
        ExprArray<IArrayExprCapsule> DestinationWithNewAliasElement = CreateDestinationWithNewAliasElement(destination);

        Context.AddToSolver(Solver, branch, Context.CreateEqualExprSet(DestinationWithNewAliasElement, DestinationWithStoredElementExpr));
    }

    private ExprArray<IArrayExprCapsule> CreateDestinationWithStoredElement(IBoolExprCapsule? branch, Variable destination, IIntExprCapsule destinationIndexExpr, IExprCapsule sourceExpr)
    {
        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);

        ExpressionType ElementType = destination.Type.ToElementType();
        IArrayRefExprCapsule ReferenceResult = Context.CreateArrayReferenceVariable(ElementType, DestinationNameAlias.ToString());

        IVariableName ArrayContainerName = CreateArrayContainerName(destination);
        Variable ArrayContainerVariable = new(ArrayContainerName, destination.Type);
        VariableAlias ArrayContainerAlias = AliasTable.GetAlias(ArrayContainerVariable);
        IArrayExprCapsule Array = Context.CreateArrayContainerVariable(ElementType, ArrayContainerAlias.ToString());

        IVariableName ArraySizeName = CreateArraySizeName(destination);
        Variable ArraySizeVariable = new(ArraySizeName, ExpressionType.Integer);
        VariableAlias ArraySizeAlias = AliasTable.GetAlias(ArraySizeVariable);
        IIntExprCapsule Size = Context.CreateIntegerVariable(ArraySizeAlias.ToString());

        Context.CreateSetElementExpr(Array, destinationIndexExpr, sourceExpr, out IArrayExprCapsule ArrayWithStore, out IBoolExprCapsule ArraySetter);
        Context.AddToSolver(Solver, branch, ArraySetter.ToSingleSet());

        ExprArray<IArrayExprCapsule> Result = new(ReferenceResult, ArrayWithStore, Size);
        return Result;
    }

    private ExprArray<IArrayExprCapsule> CreateDestinationWithNewAliasElement(Variable destination)
    {
        ExpressionType ElementType = destination.Type.ToElementType();

        AliasTable.IncrementAlias(destination);
        VariableAlias DestinationNameAlias = AliasTable.GetAlias(destination);
        IArrayRefExprCapsule ModifiedReferenceResult = Context.CreateArrayReferenceVariable(ElementType, DestinationNameAlias.ToString());

        IVariableName ArrayContainerName = CreateArrayContainerName(destination);
        Variable ArrayContainerVariable = new(ArrayContainerName, destination.Type);
        AliasTable.IncrementAlias(ArrayContainerVariable);
        VariableAlias ArrayContainerAlias = AliasTable.GetAlias(ArrayContainerVariable);
        IArrayExprCapsule ModifiedArray = Context.CreateArrayContainerVariable(ElementType, ArrayContainerAlias.ToString());

        IVariableName ArraySizeName = CreateArraySizeName(destination);
        Variable ArraySizeVariable = new(ArraySizeName, ExpressionType.Integer);
        AliasTable.IncrementAlias(ArraySizeVariable);
        VariableAlias ArraySizeAlias = AliasTable.GetAlias(ArraySizeVariable);
        IIntExprCapsule ModifiedSize = Context.CreateIntegerVariable(ArraySizeAlias.ToString());

        ExprArray<IArrayExprCapsule> Result = new(ModifiedReferenceResult, ModifiedArray, ModifiedSize);
        return Result;
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

    public void MergeBranches(Instance owner, Method hostMethod, AliasTable whenTrueAliasTable, IBoolExprCapsule branchTrue, List<VariableAlias> aliasesOnlyWhenTrue, AliasTable whenFalseAliasTable, IBoolExprCapsule branchFalse, List<VariableAlias> aliasesOnlyWhenFalse)
    {
        // Merge aliases from the if branch (the table currently contains the end of the end branch).
        AliasTable.Merge(hostMethod.Name, whenTrueAliasTable, out List<Variable> UpdatedNameList);

        AddConditionalAliases(owner, hostMethod, branchTrue, aliasesOnlyWhenFalse);
        AddConditionalAliases(owner, hostMethod, branchFalse, aliasesOnlyWhenTrue);

        foreach (Variable Variable in UpdatedNameList)
        {
            ExpressionType VariableType = Variable.Type;

            VariableAlias NameAlias = AliasTable.GetAlias(Variable);
            IExprBase<IExprCapsule, IExprCapsule> DestinationExpr = CreateVariableExpr(owner, hostMethod, NameAlias, VariableType);

            VariableAlias WhenTrueNameAlias = whenTrueAliasTable.GetAlias(Variable);
            IExprBase<IExprCapsule, IExprCapsule> WhenTrueSourceExpr = CreateVariableExpr(owner, hostMethod, WhenTrueNameAlias, VariableType);
            IExprSet<IBoolExprCapsule> WhenTrueInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenTrueSourceExpr);

            VariableAlias WhenFalseNameAlias = whenFalseAliasTable.GetAlias(Variable);
            IExprBase<IExprCapsule, IExprCapsule> WhenFalseSourceExpr = CreateVariableExpr(owner, hostMethod, WhenFalseNameAlias, VariableType);
            IExprSet<IBoolExprCapsule> WhenFalseInitExpr = Context.CreateEqualExprSet(DestinationExpr, WhenFalseSourceExpr);

            Context.AddToSolver(Solver, branchTrue, WhenTrueInitExpr);
            Context.AddToSolver(Solver, branchFalse, WhenFalseInitExpr);
        }
    }

    private void AddConditionalAliases(Instance? owner, Method? hostMethod, IBoolExprCapsule branch, List<VariableAlias> aliasList)
    {
        foreach (VariableAlias Alias in aliasList)
        {
            Variable Variable = Alias.Variable;
            ExpressionType VariableType = Variable.Type;

            IExprBase<IExprCapsule, IExprCapsule> VariableExpr = CreateVariableExpr(owner, hostMethod, Alias, VariableType);
            IExprBase<IExprCapsule, IExprCapsule> InitializerExpr = GetDefaultExpr(VariableType);
            IExprSet<IBoolExprCapsule> InitExpr = Context.CreateEqualExprSet(VariableExpr, InitializerExpr);

            Context.AddToSolver(Solver, branch, InitExpr);
        }
    }

    private IExprBase<IExprCapsule, IExprCapsule> CreateVariableExpr(Instance? owner, Method? hostMethod, VariableAlias alias, ExpressionType variableType)
    {
        Debug.Assert(variableType != ExpressionType.Other);

        string AliasString = alias.ToString();
        IExprBase<IExprCapsule, IExprCapsule> Result;

        if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, Func<string, IExprSet<IExprCapsule>>> SwitchTable = new()
            {
                { ExpressionType.Boolean, CreateBooleanVariable },
                { ExpressionType.Integer, CreateIntegerVariable },
                { ExpressionType.FloatingPoint, CreateFloatingPointVariable },
            };

            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType](AliasString);
        }
        else if (variableType.IsArray)
        {
            if (!ArrayGetterTable.ContainsKey(alias))
                GenerateArrayGetter(owner, hostMethod, alias, variableType.ToElementType());

            Result = CreateArrayReferenceVariable(alias, variableType);
        }
        else
            Result = CreateObjectReferenceVariable(alias, variableType);

        return Result;
    }

    private IExprBase<IExprCapsule, IExprCapsule> CreateInitializerExpr(ExpressionType variableType, ILiteralExpression? variableInitializer)
    {
        IExprBase<IExprCapsule, IExprCapsule> Result;

        if (variableInitializer is NewObjectExpression NewObject)
            Result = CreateObjectInitializer(NewObject.ObjectType);
        else if (variableInitializer is NewArrayExpression NewArray)
            Result = CreateArrayInitializer(NewArray.ArrayType, NewArray.ArraySize);
        else if (variableInitializer is LiteralNullExpression LiteralNull)
            Result = CreateNullInitializer(variableType);
        else if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, Func<ILiteralExpression?, IExprBase<IExprCapsule, IExprCapsule>>> SwitchTable = new()
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

    private IExprBase<IBoolExprCapsule, IBoolExprCapsule> CreateBooleanInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralBooleanValueExpression LiteralBoolean)
        {
            IBoolExprCapsule Expr = Context.CreateBooleanValue(LiteralBoolean.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.FalseSet;
    }

    private IExprBase<IIntExprCapsule, IIntExprCapsule> CreateIntegerInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralIntegerValueExpression LiteralInteger)
        {
            IIntExprCapsule Expr = Context.CreateIntegerValue(LiteralInteger.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    private IExprBase<IArithExprCapsule, IArithExprCapsule> CreateFloatingPointInitializer(ILiteralExpression? variableInitializer)
    {
        if (variableInitializer is LiteralFloatingPointValueExpression LiteralFloatingPoint)
        {
            IArithExprCapsule Expr = Context.CreateFloatingPointValue(LiteralFloatingPoint.Value);
            return Expr.ToSingleSet();
        }
        else
            return Context.ZeroSet;
    }

    public IExprBase<IRefExprCapsule, IExprCapsule> CreateObjectInitializer(ExpressionType expressionType)
    {
        ClassModel TypeClassModel = TypeToModel(expressionType);
        IObjectRefExprCapsule ReferenceResult = Context.CreateObjectReferenceValue(TypeClassModel.ClassName, Index.Increment());

        return CreateObjectInitializer(expressionType, ReferenceResult);
    }

    public ExprObject CreateObjectInitializer(ExpressionType expressionType, IRefExprCapsule referenceResult)
    {
        Debug.Assert(!expressionType.IsArray);

        List<IExprBase<IExprCapsule, IExprCapsule>> VariableSetList = new();

        ClassModel TypeClassModel = TypeToModel(expressionType);

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;
            IExprBase<IExprCapsule, IExprCapsule> PropertyExpressions = CreateInitializerExpr(Property.Type, Property.Initializer);
            VariableSetList.Add(PropertyExpressions);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in TypeClassModel.FieldTable)
        {
            Field Field = Entry.Value;
            IExprBase<IExprCapsule, IExprCapsule> FieldExpressions = CreateInitializerExpr(Field.Type, Field.Initializer);
            VariableSetList.Add(FieldExpressions);
        }

        ExprObject Result = new(referenceResult, VariableSetList);

        return Result;
    }

    public IExprBase<IRefExprCapsule, IExprCapsule> CreateArrayInitializer(ExpressionType expressionType, ArraySize arraySize)
    {
        Debug.Assert(expressionType.IsArray);
        Debug.Assert(arraySize.IsKnown);

        ExpressionType ElementType = expressionType.ToElementType();
        IArrayRefExprCapsule ReferenceResult = Context.CreateArrayReferenceValue(ElementType, Index.Increment());

        return CreateArrayInitializer(ElementType, arraySize, ReferenceResult);
    }

    public ExprArray<IArrayExprCapsule> CreateArrayInitializer(ExpressionType elementType, ArraySize arraySize, IRefExprCapsule referenceResult)
    {
        IArrayExprCapsule Array = Context.CreateArrayValue(elementType, arraySize);
        IIntExprCapsule Size = Context.CreateIntegerValue(arraySize.Size);
        ExprArray<IArrayExprCapsule> Result = new(referenceResult, Array, Size);

        return Result;
    }

    public IExprBase<IRefExprCapsule, IExprCapsule> CreateNullInitializer(ExpressionType expressionType)
    {
        if (expressionType.IsArray)
            return CreateArrayInitializer(expressionType.ToElementType(), ArraySize.Unknown, Context.Null);
        else
            return CreateObjectInitializer(expressionType, Context.Null);
    }

    private IExprSet<IBoolExprCapsule> CreateBooleanVariable(string alias)
    {
        return Context.CreateBooleanVariable(alias).ToSingleSet();
    }

    private IExprSet<IIntExprCapsule> CreateIntegerVariable(string alias)
    {
        return Context.CreateIntegerVariable(alias).ToSingleSet();
    }

    private IExprSet<IArithExprCapsule> CreateFloatingPointVariable(string alias)
    {
        return Context.CreateFloatingPointVariable(alias).ToSingleSet();
    }

    public ExprObject CreateObjectReferenceVariable(VariableAlias alias, ExpressionType variableType)
    {
        ClassModel ReferenceClassModel = TypeToModel(variableType);
        IObjectRefExprCapsule ReferenceResult = Context.CreateObjectReferenceVariable(ReferenceClassModel.ClassName, alias.ToString());
        Instance Reference = new() { ClassModel = ReferenceClassModel, Expr = ReferenceResult };
        List<IExprBase<IExprCapsule, IExprCapsule>> VariableSetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in ReferenceClassModel.PropertyTable)
        {
            Property Property = Entry.Value;
            Variable PropertyVariable = CreateAliasTrackedVariable(Reference, hostMethod: null, Property, Property.Type);

            if (!AliasTable.ContainsVariable(PropertyVariable))
                AliasTable.AddVariable(PropertyVariable);

            VariableAlias PropertyVariableNameAlias = AliasTable.GetAlias(PropertyVariable);

            IExprBase<IExprCapsule, IExprCapsule> PropertyExpressions = CreateVariableExpr(Reference, hostMethod: null, PropertyVariableNameAlias, Property.Type);
            VariableSetList.Add(PropertyExpressions);
        }

        foreach (KeyValuePair<FieldName, Field> Entry in ReferenceClassModel.FieldTable)
        {
            Field Field = Entry.Value;
            Variable FieldVariable = CreateAliasTrackedVariable(Reference, hostMethod: null, Field, Field.Type);

            if (!AliasTable.ContainsVariable(FieldVariable))
                AliasTable.AddVariable(FieldVariable);

            VariableAlias FieldVariableNameAlias = AliasTable.GetAlias(FieldVariable);

            IExprBase<IExprCapsule, IExprCapsule> FieldExpressions = CreateVariableExpr(Reference, hostMethod: null, FieldVariableNameAlias, Field.Type);
            VariableSetList.Add(FieldExpressions);
        }

        ExprObject Result = new(ReferenceResult, VariableSetList);

        return Result;
    }

    private ExprArray<IArrayExprCapsule> CreateArrayReferenceVariable(VariableAlias alias, ExpressionType variableType)
    {
        ExpressionType ElementType = variableType.ToElementType();
        IArrayRefExprCapsule ReferenceResult = Context.CreateArrayReferenceVariable(ElementType, alias.ToString());

        IVariableName ArrayContainerName = CreateArrayContainerName(alias.Variable);
        Variable ArrayContainerVariable = new(ArrayContainerName, variableType);

        if (!AliasTable.ContainsVariable(ArrayContainerVariable))
            AliasTable.AddVariable(ArrayContainerVariable);

        VariableAlias ArrayContainerAlias = AliasTable.GetAlias(ArrayContainerVariable);
        IArrayExprCapsule Array = Context.CreateArrayContainerVariable(ElementType, ArrayContainerAlias.ToString());

        IVariableName ArraySizeName = CreateArraySizeName(alias.Variable);
        Variable ArraySizeVariable = new(ArraySizeName, ExpressionType.Integer);

        if (!AliasTable.ContainsVariable(ArraySizeVariable))
            AliasTable.AddVariable(ArraySizeVariable);

        VariableAlias ArraySizeAlias = AliasTable.GetAlias(ArraySizeVariable);

        IIntExprCapsule Size = Context.CreateIntegerVariable(ArraySizeAlias.ToString());

        ExprArray<IArrayExprCapsule> Result = new(ReferenceResult, Array, Size);

        return Result;
    }

    public Local FindOrCreateResultLocal(Method hostMethod, ExpressionType returnType)
    {
        foreach (KeyValuePair<LocalName, Local> Entry in hostMethod.RootBlock.LocalTable)
            if (Entry.Key.Text == Ensure.ResultKeyword)
                return Entry.Value;

        Local? ResultLocal = hostMethod.ResultLocal as Local;
        Debug.Assert(ResultLocal is not null);

        Variable ResultLocalVariable = CreateAliasTrackedVariable(owner: null, hostMethod, ResultLocal!, returnType);

        AliasTable.AddOrIncrement(ResultLocalVariable);

        return ResultLocal!;
    }

    public static string GetBlockNameText(Instance? owner, Method? hostMethod, IVariable variable)
    {
        string Result = null!;

        if (hostMethod is not null)
            Result = GetLocalBlockNameText(hostMethod, variable);

        if (owner is not null)
            Result = GetOwnerBlockNameText(owner, variable);

        Debug.Assert(Result is not null);

        return Result!;
    }

    public static string GetLocalBlockNameText(Method hostMethod, IVariable variable)
    {
        string LocalBlockText = $"{hostMethod.ClassName}::{hostMethod.Name.Text}-${variable.Name.Text}";
        return LocalBlockText;
    }

    public static string GetOwnerBlockNameText(Instance owner, IVariable variable)
    {
        Debug.Assert(owner.Expr is IObjectRefExprCapsule);

        IObjectRefExprCapsule ObjectExpr = (IObjectRefExprCapsule)owner.Expr;
        ClassName ClassName = ObjectExpr.ClassName;

        Debug.Assert(ClassName != ClassName.Empty);

        string OwnerText = $"{ClassName}#{ObjectExpr.Index.Internal}:${variable.Name.Text}";

        return OwnerText;
    }

    public static LocalName CreateArrayContainerName(IVariable variable)
    {
        string OwnerText = $"{variable.Name.Text}[]-Container";

        return new LocalName() { Text = OwnerText };
    }

    public static LocalName CreateArraySizeName(IVariable variable)
    {
        string OwnerText = $"{variable.Name.Text}[]-Size";

        return new LocalName() { Text = OwnerText };
    }

    public IExprBase<IExprCapsule, IExprCapsule> GetDefaultExpr(ExpressionType variableType)
    {
        IExprBase<IExprCapsule, IExprCapsule> Result;

        if (variableType.IsSimple)
        {
            Dictionary<ExpressionType, IExprBase<IExprCapsule, IExprCapsule>> SwitchTable = new()
            {
                { ExpressionType.Boolean, Context.FalseSet },
                { ExpressionType.Integer, Context.ZeroSet },
                { ExpressionType.FloatingPoint, Context.ZeroSet },
            };

            Debug.Assert(SwitchTable.ContainsKey(variableType));
            Result = SwitchTable[variableType];
        }
        else
        {
            Debug.Assert(!variableType.IsArray);

            Result = CreateObjectNullDefault(variableType);
        }

        return Result;
    }

    public ExprObject CreateObjectNullDefault(ExpressionType variableType)
    {
        ClassModel TypeClassModel = TypeToModel(variableType);
        List<IExprBase<IExprCapsule, IExprCapsule>> PropertySetList = new();

        foreach (KeyValuePair<PropertyName, Property> Entry in TypeClassModel.PropertyTable)
        {
            Property Property = Entry.Value;

            IExprBase<IExprCapsule, IExprCapsule> PropertyExpressions = GetDefaultExpr(Property.Type);
            PropertySetList.Add(PropertyExpressions);
        }

        ExprObject Result = new(Context.Null, PropertySetList);

        return Result;
    }

    public void ClearState(Instance reference)
    {
        foreach (KeyValuePair<PropertyName, Property> Entry in reference.ClassModel.PropertyTable)
        {
            Variable PropertyVariable = CreateAliasTrackedVariable(reference, hostMethod: null, Entry.Value, Entry.Value.Type);

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

    public IExprSingle<IIntExprCapsule> InitializeIndexRun(Method hostMethod, IBoolExprCapsule? branch, Local localIndex, IExprBase<IExprCapsule, IExprCapsule> initIndexExpr)
    {
        Variable IndexVariable = CreateAliasTrackedVariable(owner: null, hostMethod, localIndex, localIndex.Type);
        AliasTable.IncrementAlias(IndexVariable);

        VariableAlias VaryingIndexNameAlias = AliasTable.GetAlias(IndexVariable);
        IExprBase<IExprCapsule, IExprCapsule> VaryingIndexExpr = CreateVariableExpr(owner: null, hostMethod, VaryingIndexNameAlias, localIndex.Type);

        Debug.Assert(VaryingIndexExpr is IExprSingle<IIntExprCapsule>);
        Debug.Assert(initIndexExpr is IExprSingle<IIntExprCapsule>);

        IExprSingle<IIntExprCapsule> VaryingIndexSingle = (IExprSingle<IIntExprCapsule>)VaryingIndexExpr;
        IExprSingle<IIntExprCapsule> InitIndexSingle = (IExprSingle<IIntExprCapsule>)initIndexExpr;

        IBoolExprCapsule ComparisonExpr = Context.CreateGreaterThanEqualToExpr(VaryingIndexSingle.MainExpression, InitIndexSingle.MainExpression);
        IExprSingle<IBoolExprCapsule> InitExpr = ComparisonExpr.ToSingleSet();
        Context.AddToSolver(Solver, branch, InitExpr);

        return VaryingIndexSingle;
    }

    private void GenerateArrayGetter(Instance? owner, Method? hostMethod, VariableAlias alias, ExpressionType elementType)
    {
        ClassName ClassName = null!;

        if (hostMethod is not null)
            ClassName = hostMethod.ClassName;

        if (owner is not null)
            ClassName = owner.ClassModel.ClassName;

        MethodName MethodName = new MethodName() { Text = $"{alias}_get" };
        ParameterTable ParameterTable = new();
        ParameterName ParameterName = new() { Text = "i" };
        Parameter Parameter = new(ParameterName, ExpressionType.Integer) { ClassName = ClassName, MethodName = MethodName };
        ParameterTable.AddItem(Parameter);

        Dictionary<ExpressionType, ILiteralExpression> ZeroTable = new()
        {
            { ExpressionType.Boolean, new LiteralBooleanValueExpression() { Value = false } },
            { ExpressionType.Integer, new LiteralIntegerValueExpression() { Value = 0 } },
            { ExpressionType.FloatingPoint, new LiteralFloatingPointValueExpression() { Value = 0 } },
        };

        Debug.Assert(ZeroTable.ContainsKey(elementType));
        ReturnStatement Return = new() { Expression = (Expression)ZeroTable[elementType] };

        BlockScope RootBlock = new() { IndexLocal = null, ContinueCondition = null, LocalTable = new(), StatementList = new() { Return } };

        Method NewMethod = new()
        {
            Name = MethodName,
            ClassName = ClassName,
            AccessModifier = AccessModifier.Private,
            IsStatic = false,
            IsPreloaded = false,
            ReturnType = elementType,
            ParameterTable = new ReadOnlyParameterTable(ParameterTable),
            RequireList = new(),
            RootBlock = RootBlock,
            EnsureList = new(),
        };

        Debug.Assert(!ArrayGetterTable.ContainsKey(alias));
        ArrayGetterTable.Add(alias, NewMethod);
    }

    public Method GetArrayGetter(Instance? owner, Method? hostMethod, IVariable variable)
    {
        Variable Variable = CreateAliasTrackedVariable(owner, hostMethod, variable, variable.Type);
        VariableAlias VariableNameAlias = AliasTable.GetAlias(Variable);

        Debug.Assert(ArrayGetterTable.ContainsKey(VariableNameAlias));
        return ArrayGetterTable[VariableNameAlias];
    }

    public void GenerateModifiedGetter(Instance? owner, Method? hostMethod, IVariable variable, LiteralIntegerValueExpression literalIntegerValue, Expression newValueExpression)
    {
        Variable Variable = CreateAliasTrackedVariable(owner, hostMethod, variable, variable.Type);

        VariableAlias OldVariableNameAlias = AliasTable.GetAlias(Variable);
        Debug.Assert(ArrayGetterTable.ContainsKey(OldVariableNameAlias));
        Method OldMethod = ArrayGetterTable[OldVariableNameAlias];

        AliasTable.IncrementAlias(Variable);
        VariableAlias NewVariableNameAlias = AliasTable.GetAlias(Variable);

        ClassName ClassName = null!;

        if (hostMethod is not null)
            ClassName = hostMethod.ClassName;

        if (owner is not null)
            ClassName = owner.ClassModel.ClassName;

        MethodName MethodName = new MethodName() { Text = $"{NewVariableNameAlias}_get" };

        ParameterTable ParameterTable = new();
        ParameterName ParameterName = new() { Text = "i" };
        Parameter Parameter = new(ParameterName, ExpressionType.Integer) { ClassName = ClassName, MethodName = MethodName };
        ParameterTable.AddItem(Parameter);

        VariableValueExpression VariableValue = new() { PathLocation = null!, VariablePath = new List<IVariable>() { Parameter } };
        EqualityExpression Equality = new() { Left = VariableValue, Operator = EqualityOperator.Equal, Right = literalIntegerValue };

        GenerateModifiedGetter(owner, hostMethod, NewVariableNameAlias, variable.Type, ClassName, MethodName, Parameter, Equality, newValueExpression, OldMethod);
    }

    public void GenerateModifiedGetter(Instance? owner, Method? hostMethod, IVariable variable, Expression continueCondition, Expression newValueExpression)
    {
        Variable Variable = CreateAliasTrackedVariable(owner, hostMethod, variable, variable.Type);

        VariableAlias OldVariableNameAlias = AliasTable.GetAlias(Variable);
        Debug.Assert(ArrayGetterTable.ContainsKey(OldVariableNameAlias));
        Method OldMethod = ArrayGetterTable[OldVariableNameAlias];

        AliasTable.IncrementAlias(Variable);
        VariableAlias NewVariableNameAlias = AliasTable.GetAlias(Variable);

        ClassName ClassName = null!;

        if (hostMethod is not null)
            ClassName = hostMethod.ClassName;

        if (owner is not null)
            ClassName = owner.ClassModel.ClassName;

        MethodName MethodName = new MethodName() { Text = $"{NewVariableNameAlias}_get" };

        ParameterTable ParameterTable = new();
        ParameterName ParameterName = new() { Text = "i" };
        Parameter Parameter = new(ParameterName, ExpressionType.Integer) { ClassName = ClassName, MethodName = MethodName };
        ParameterTable.AddItem(Parameter);

        GenerateModifiedGetter(owner, hostMethod, NewVariableNameAlias, variable.Type, ClassName, MethodName, Parameter, continueCondition, newValueExpression, OldMethod);
    }

    public void GenerateModifiedGetter(Instance? owner, Method? hostMethod, VariableAlias alias, ExpressionType variableType, ClassName className, MethodName methodName, Parameter parameter, Expression comparisonExpression, Expression newValueExpression, Method oldMethod)
    {
        ParameterTable ParameterTable = new();
        ParameterTable.AddItem(parameter);

        ExpressionType ElementType = variableType.ToElementType();

        Local ResultLocal = new(new LocalName() { Text = Ensure.ResultKeyword }, ElementType) { Initializer = null, ClassName = className, MethodName = methodName };
        LocalTable LocalTable = new();
        LocalTable.AddItem(ResultLocal);

        VariableValueExpression VariableValue = new() { PathLocation = null!, VariablePath = new List<IVariable>() { parameter } };
        AssignmentStatement AssignWhenTrue = new() { DestinationName = ResultLocal.Name, Expression = newValueExpression, DestinationIndex = null };
        BlockScope WhenTrueBlock = new() { IndexLocal = null, ContinueCondition = null, LocalTable = new(), StatementList = new List<Statement>() { AssignWhenTrue } };
        Argument Argument = new() { Expression = VariableValue, Location = null! };
        PrivateFunctionCallExpression FunctionCall = new() { CallerClassName = className, ClassName = className, MethodName = oldMethod.Name, ReturnType = variableType, ArgumentList = new List<Argument>() { Argument }, NameLocation = null! };
        AssignmentStatement AssignWhenFalse = new() { DestinationName = ResultLocal.Name, Expression = FunctionCall, DestinationIndex = null };
        BlockScope WhenFalseBlock = new() { IndexLocal = null, ContinueCondition = null, LocalTable = new(), StatementList = new List<Statement>() { AssignWhenFalse } };
        ConditionalStatement Conditional = new() { Condition = comparisonExpression, WhenTrueBlock = WhenTrueBlock, WhenFalseBlock = WhenFalseBlock };

        VariableValueExpression ResultExpression = new() { VariablePath = new List<IVariable>() { ResultLocal }, PathLocation = Location.None, LocationId = null! };
        ReturnStatement Return = new() { Expression = ResultExpression };

        BlockScope RootBlock = new() { IndexLocal = null, ContinueCondition = null, LocalTable = new(), StatementList = new() { Conditional, Return } };

        Method NewMethod = new()
        {
            Name = methodName,
            ClassName = className,
            AccessModifier = AccessModifier.Private,
            IsStatic = false,
            IsPreloaded = false,
            ReturnType = ElementType,
            ParameterTable = new ReadOnlyParameterTable(ParameterTable),
            RequireList = new(),
            RootBlock = RootBlock,
            EnsureList = new(),
        };

        Debug.Assert(!ArrayGetterTable.ContainsKey(alias));
        ArrayGetterTable.Add(alias, NewMethod);
    }

    private ReferenceIndex Index;
}
