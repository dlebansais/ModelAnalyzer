namespace ModelAnalyzer;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AnalysisLogger;
using FileExtractor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a manager for class models.
/// </summary>
public partial class ClassModelManager : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    public ClassModelManager()
        : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelManager"/> class.
    /// </summary>
    /// <param name="receiveChannelGuid">The guid of the channel receiving ack from the verifier.</param>
    public ClassModelManager(Guid receiveChannelGuid)
    {
        ReceiveChannelGuid = receiveChannelGuid;

        Extractor.Extract();
        VerificationState VerificationState = new()
        {
            ModelExchange = new ModelExchange() { ClassModelTable = new ClassModelTable(), ReceiveChannelGuid = receiveChannelGuid },
            IsVerificationRequestSent = false,
            VerificationResultTable = new Dictionary<ClassName, VerificationResult>(),
        };

        Context = new SynchronizedVerificationContext() { VerificationState = VerificationState };
        FromServerChannel = InitChannel();
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    public IAnalysisLogger Logger { get; init; } = new NullLogger();

    /// <summary>
    /// Gets or sets the thread start mode.
    /// </summary>
    public VerificationProcessStartMode StartMode { get; set; }

    /// <summary>
    /// Gets the guid of the channel receiving ack from the verifier.
    /// </summary>
    public Guid ReceiveChannelGuid { get; }

    /// <summary>
    /// Checks whether a class is ignored for modeling.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    public static bool IsClassIgnoredForModeling(ClassDeclarationSyntax classDeclaration)
    {
        string ClassName = classDeclaration.Identifier.ValueText;

        return ClassName == string.Empty || IsClassWithNoModelPrefix(classDeclaration);
    }

    private static bool IsClassWithNoModelPrefix(ClassDeclarationSyntax classDeclaration)
    {
        SyntaxToken firstToken = classDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;

        foreach (SyntaxTrivia Trivia in leadingTrivia)
            if (Trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string Comment = Trivia.ToFullString();
                if (Comment.StartsWith($"// {Modeling.None}"))
                    return true;
            }

        return false;
    }

    /// <summary>
    /// Gets models of a classes.
    /// </summary>
    /// <param name="compilationContext">The compilation context.</param>
    /// <param name="classDeclarationList">The list of class declaration.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <exception cref="ArgumentException">Empty class name.</exception>
    public IDictionary<ClassDeclarationSyntax, IClassModel> GetClassModels(CompilationContext compilationContext, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        if (classDeclarationList.Count == 0)
            return new Dictionary<ClassDeclarationSyntax, IClassModel>();

        string ClassNames = string.Empty;

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            string ClassName = ClassDeclaration.Identifier.ValueText;
            if (ClassName == string.Empty)
                throw new ArgumentException("Class name must not be empty.");

            if (ClassNames != string.Empty)
                ClassNames += ", ";

            ClassNames += ClassName;
        }

        // We clear logs only after the constructor has exited.
        ClearLogs();

        Log($"Getting model for class '{ClassNames}'.");

        return GetClassModelListInternal(compilationContext, classDeclarationList, semanticModel);
    }

    /// <summary>
    /// Gets the verified version of a class model synchronously.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public IClassModel GetVerifiedModel(IClassModel classModel)
    {
        return WaitForVerification(classModel);
    }

    /// <summary>
    /// Gets the verified version of a class model synchronously.
    /// </summary>
    /// <param name="classModel">The class model.</param>
    public async Task<IClassModel> GetVerifiedModelAsync(IClassModel classModel)
    {
        return await Task.Run(() =>
        {
            return WaitForVerification(classModel);
        });
    }

    /// <summary>
    /// Waits for the verifier to be ready to receive data.
    /// </summary>
    public async Task WaitReady()
    {
        await Task.Run(() =>
        {
            WaitReadySynchronous();
        });
    }

    /// <summary>
    /// Removes classes that no longer exist.
    /// </summary>
    /// <param name="existingClassList">The list of existing classes.</param>
    public void RemoveMissingClasses(List<ClassName> existingClassList)
    {
        Log("Cleaning up classes that no longer exist.");

        lock (Context.Lock)
        {
            List<ClassName> ToRemoveClassList = new();

            foreach (ClassName ClassName in Context.GetClassModelNameList())
                if (!existingClassList.Contains(ClassName))
                    ToRemoveClassList.Add(ClassName);

            foreach (ClassName ClassName in ToRemoveClassList)
            {
                Log($"Removing class '{ClassName}'.");

                Context.RemoveClass(ClassName);
            }
        }
    }

    private IDictionary<ClassDeclarationSyntax, IClassModel> GetClassModelListInternal(CompilationContext compilationContext, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        Dictionary<ClassDeclarationSyntax, IClassModel> ClassDeclarationModelTable = new();

        lock (Context.Lock)
        {
            Log($"Compilation context: {compilationContext}");

            // Compare this compilation context with the previous one. They will be different if their hash code is not the same, or if the new context is an asynchronous request.
            bool IsNewCompilationContext = !Context.LastCompilationContext.IsCompatibleWith(compilationContext);
            bool ClassModelAlreadyExistForAll = classDeclarationList.TrueForAll(classDeclaration => Context.ContainsClass(semanticModel.ClassDeclarationToClassName(classDeclaration)));

            if (IsNewCompilationContext || !ClassModelAlreadyExistForAll)
            {
                VerificationState OldVerificationState = Context.VerificationState;
                ModelExchange OldClassModelExchange = OldVerificationState.ModelExchange;
                Dictionary<ClassName, IClassModel> Phase1ClassModelTable = new();
                ClassModelTable OldClassModelTable = OldClassModelExchange.ClassModelTable;
                ClassModelTable NewClassModelTable = new();

                ParsePhase1(Phase1ClassModelTable, classDeclarationList, semanticModel);
                semanticModel.Phase1ClassModelTable = Phase1ClassModelTable;
                ParsePhase2(OldClassModelTable, NewClassModelTable, ClassDeclarationModelTable, classDeclarationList, semanticModel);

                ReportCyclicReferences(NewClassModelTable);

                ModelExchange NewClassModelExchange = OldClassModelExchange with
                {
                    ClassModelTable = NewClassModelTable,
                };

                VerificationState NewVerificationState = OldVerificationState with
                {
                    ModelExchange = NewClassModelExchange,
                    IsVerificationRequestSent = false,
                    VerificationResultTable = new Dictionary<ClassName, VerificationResult>(),
                };

                Context.VerificationState = NewVerificationState;

                if (StartMode == VerificationProcessStartMode.Auto)
                    ScheduleAsynchronousVerification();
            }
            else
            {
                ClassModelTable ClassModelTable = Context.VerificationState.ModelExchange.ClassModelTable;

                foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
                {
                    ClassName ClassName = semanticModel.ClassDeclarationToClassName(ClassDeclaration);

                    Debug.Assert(ClassName != ClassName.Empty);
                    Debug.Assert(ClassModelTable.ContainsKey(ClassName));

                    ClassDeclarationModelTable.Add(ClassDeclaration, ClassModelTable[ClassName]);
                }
            }

            Context.LastCompilationContext = compilationContext;
        }

        return ClassDeclarationModelTable;
    }

    private void ParsePhase1(Dictionary<ClassName, IClassModel> phase1ClassModelTable, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        ClassModelTable PreloadedClasses = GetPreloadedClasses();
        foreach (KeyValuePair<ClassName, ClassModel> Entry in PreloadedClasses)
            phase1ClassModelTable.Add(Entry.Key, Entry.Value);

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            ClassModel NewClassModel = ParseClassModelPhase1(classDeclarationList, ClassDeclaration, semanticModel);
            phase1ClassModelTable.Add(NewClassModel.ClassName, NewClassModel);
        }
    }

    private ClassModel ParseClassModelPhase1(List<ClassDeclarationSyntax> classDeclarationList, ClassDeclarationSyntax classDeclaration, IModel semanticModel)
    {
        ClassName ClassName = semanticModel.ClassDeclarationToClassName(classDeclaration);
        Debug.Assert(ClassName != ClassName.Empty);

        ClassDeclarationParser Parser = new(classDeclarationList, classDeclaration, semanticModel) { Logger = Logger };
        Parser.ParsePhase1();

        ClassModel ClassModel = new ClassModel()
        {
            ClassName = ClassName,
            PropertyTable = Parser.PropertyTable,
            FieldTable = Parser.FieldTable,
            MethodTable = Parser.MethodTable,
            InvariantList = Parser.InvariantList,
            Unsupported = Parser.Unsupported,
            InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
            RequireViolations = new List<IRequireViolation>().AsReadOnly(),
            EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
            AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
        };

        return ClassModel;
    }

    private void ParsePhase2(ClassModelTable oldClassModelTable, ClassModelTable newClassModelTable, Dictionary<ClassDeclarationSyntax, IClassModel> classDeclarationModelTable, List<ClassDeclarationSyntax> classDeclarationList, IModel semanticModel)
    {
        ClassModelTable PreloadedClasses = GetPreloadedClasses();
        foreach (KeyValuePair<ClassName, ClassModel> Entry in PreloadedClasses)
            newClassModelTable.Add(Entry.Key, Entry.Value);

        MethodCallStatementList.Clear();
        FunctionCallExpressionList.Clear();
        ArithmeticExpressionList.Clear();

        foreach (ClassDeclarationSyntax ClassDeclaration in classDeclarationList)
        {
            ClassModel NewClassModel = AddOrUpdateClassModelPhase2(oldClassModelTable, newClassModelTable, classDeclarationList, ClassDeclaration, semanticModel);
            classDeclarationModelTable.Add(ClassDeclaration, NewClassModel);
        }
    }

    private ClassModel AddOrUpdateClassModelPhase2(ClassModelTable oldClassModelTable, ClassModelTable newClassModelTable, List<ClassDeclarationSyntax> classDeclarationList, ClassDeclarationSyntax classDeclaration, IModel semanticModel)
    {
        ClassName ClassName = semanticModel.ClassDeclarationToClassName(classDeclaration);
        Debug.Assert(ClassName != ClassName.Empty);

        ClassDeclarationParser Parser = new(classDeclarationList, classDeclaration, semanticModel) { Logger = Logger };
        Parser.ParsePhase2();

        ClassModel NewClassModel;

        if (oldClassModelTable.ContainsKey(ClassName))
        {
            Log($"Updating model for class '{ClassName}'.");

            ClassModel OldClassModel = oldClassModelTable[ClassName];

            NewClassModel = OldClassModel with
            {
                PropertyTable = Parser.PropertyTable,
                FieldTable = Parser.FieldTable,
                MethodTable = Parser.MethodTable,
                InvariantList = Parser.InvariantList,
                Unsupported = Parser.Unsupported,
            };
        }
        else
        {
            Log($"Adding new model for class '{ClassName}'.");

            NewClassModel = new ClassModel()
            {
                ClassName = ClassName,
                PropertyTable = Parser.PropertyTable,
                FieldTable = Parser.FieldTable,
                MethodTable = Parser.MethodTable,
                InvariantList = Parser.InvariantList,
                Unsupported = Parser.Unsupported,
                InvariantViolations = new List<IInvariantViolation>().AsReadOnly(),
                RequireViolations = new List<IRequireViolation>().AsReadOnly(),
                EnsureViolations = new List<IEnsureViolation>().AsReadOnly(),
                AssumeViolations = new List<IAssumeViolation>().AsReadOnly(),
            };
        }

        newClassModelTable.Add(ClassName, NewClassModel);

        MethodCallStatementList.AddRange(Parser.MethodCallStatementList);
        FunctionCallExpressionList.AddRange(Parser.FunctionCallExpressionList);
        ArithmeticExpressionList.AddRange(Parser.ArithmeticExpressionList);

        return NewClassModel;
    }

    private ClassModelTable GetPreloadedClasses()
    {
        ClassModelTable PreloadedClasses = Preloaded.GetClasses();
        ClassModelTable Result = new();

        foreach (KeyValuePair<ClassName, ClassModel> Entry in PreloadedClasses)
        {
            ClassModel ClassModel = Entry.Value;
            FixPreloadedClassModelClauses(ClassModel);

            Result.Add(ClassModel.ClassName, ClassModel);
        }

        return Result;
    }

    private void FixPreloadedClassModelClauses(ClassModel classModel)
    {
        foreach (KeyValuePair<MethodName, Method> Entry in classModel.MethodTable)
        {
            Method Method = Entry.Value;

            for (int i = 0; i < Method.RequireList.Count; i++)
            {
                Require Require = Method.RequireList[i];
                CallRequireLocation CallLocation = new() { ParentRequireList = Method.RequireList, RequireIndex = i };
                Method.RequireList[i] = FixedRequireClause(classModel, Method, Require, CallLocation);
            }

            for (int i = 0; i < Method.EnsureList.Count; i++)
            {
                Ensure Ensure = Method.EnsureList[i];
                CallEnsureLocation CallLocation = new() { ParentEnsureList = Method.EnsureList, EnsureIndex = i };
                Method.EnsureList[i] = FixedEnsureClause(classModel, Method, Ensure, CallLocation);
            }
        }
    }

    private Require FixedRequireClause(ClassModel classModel, Method method, Require require, CallRequireLocation callLocation)
    {
        string Text = require.Text;
        Expression BooleanExpression = PreloadedClauseTextToExpression(classModel, method, Text, callLocation);

        Debug.Assert(BooleanExpression.GetExpressionType() == ExpressionType.Boolean);

        return new Require()
        {
            Text = Text,
            Location = Location.None,
            BooleanExpression = BooleanExpression,
        };
    }

    private Ensure FixedEnsureClause(ClassModel classModel, Method method, Ensure ensure, CallEnsureLocation callLocation)
    {
        string Text = ensure.Text;
        Expression BooleanExpression = PreloadedClauseTextToExpression(classModel, method, Text, callLocation);

        Debug.Assert(BooleanExpression.GetExpressionType() == ExpressionType.Boolean);

        return new Ensure()
        {
            Text = Text,
            Location = Location.None,
            BooleanExpression = BooleanExpression,
        };
    }

    private Expression PreloadedClauseTextToExpression(ClassModel classModel, Method method, string text, ICallLocation callLocation)
    {
        MadeUpSemanticModel SemanticModel = new();
        SemanticModel.Phase1ClassModelTable.Add(classModel.ClassName, classModel);

        ClassDeclarationParser Parser = new(new List<ClassDeclarationSyntax>(), null!, SemanticModel);
        Expression? Expression = Parser.ParseAssertionText(method, text, callLocation);

        Debug.Assert(Expression is not null);

        return Expression!;
    }

    private void ReportCyclicReferences(ClassModelTable classModelTable)
    {
        foreach (KeyValuePair<ClassName, ClassModel> Entry in classModelTable)
            if (IsCycleDetected(classModelTable, Entry.Value))
                break;
    }

    private bool IsCycleDetected(ClassModelTable classModelTable, ClassModel classModel)
    {
        List<ClassName> VisitedClasses = new() { classModel.ClassName };

        return IsCycleDetected(classModelTable, VisitedClasses, classModel);
    }

    private bool IsCycleDetected(ClassModelTable classModelTable, List<ClassName> visitedClasses, ClassModel classModel)
    {
        if (!classModel.Unsupported.IsEmpty)
            return false;

        foreach (KeyValuePair<PropertyName, Property> Entry in classModel.PropertyTable)
        {
            Property Property = Entry.Value;
            if (IsCycleDetected(classModelTable, visitedClasses, classModel, Property.Type))
                return true;
        }

        foreach (KeyValuePair<FieldName, Field> Entry in classModel.FieldTable)
        {
            Field Field = Entry.Value;
            if (IsCycleDetected(classModelTable, visitedClasses, classModel, Field.Type))
                return true;
        }

        foreach (KeyValuePair<MethodName, Method> Entry in classModel.MethodTable)
        {
            Method Method = Entry.Value;
            if (IsCycleDetected(classModelTable, visitedClasses, classModel, Method.ReturnType))
                return true;

            foreach (KeyValuePair<ParameterName, Parameter> ParameterEntry in Method.ParameterTable)
            {
                Parameter Parameter = ParameterEntry.Value;
                if (IsCycleDetected(classModelTable, visitedClasses, classModel, Parameter.Type))
                    return true;
            }

            foreach (KeyValuePair<LocalName, Local> LocalEntry in Method.LocalTable)
            {
                Local Local = LocalEntry.Value;
                if (IsCycleDetected(classModelTable, visitedClasses, classModel, Local.Type))
                    return true;
            }
        }

        return false;
    }

    private bool IsCycleDetected(ClassModelTable classModelTable, List<ClassName> visitedClasses, ClassModel classModel, ExpressionType variableType)
    {
        ExpressionType Type = ArrayOrElementOfArray(variableType);

        if (!Type.IsSimple)
        {
            ClassName VisitedClassName = Type.TypeName;

            if (visitedClasses.Contains(VisitedClassName))
            {
                classModel.Unsupported.IsPartOfCycle = true;
                return true;
            }

            Debug.Assert(classModelTable.ContainsKey(VisitedClassName));

            ClassModel VisitedClassModel = classModelTable[VisitedClassName];
            List<ClassName> NewVisitedClasses = new();
            NewVisitedClasses.AddRange(visitedClasses);
            NewVisitedClasses.Add(VisitedClassName);

            if (IsCycleDetected(classModelTable, NewVisitedClasses, VisitedClassModel))
                return true;
        }

        return false;
    }

    private ExpressionType ArrayOrElementOfArray(ExpressionType variableType)
    {
        if (variableType.IsArray)
            return variableType.ToElementType();
        else
            return variableType;
    }

    private void Log(string message)
    {
        Logger.Log(message);
    }

    private void ClearLogs()
    {
        lock (Context.Lock)
        {
            if (Context.VerificationState.ModelExchange.ClassModelTable.Count == 0)
                Logger.Clear();
        }
    }

    private SynchronizedVerificationContext Context;
}
